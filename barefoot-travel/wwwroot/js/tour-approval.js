// Tour Status Constants - matching backend constants
const TourStatus = {
    DRAFT: 'draft',
    PUBLIC: 'public',
    HIDE: 'hide',
    CANCELLED: 'cancelled'
};

// Status Display Names
const TourStatusDisplayNames = {
    'draft': 'Draft',
    'public': 'Public',
    'hide': 'Hidden',
    'cancelled': 'Cancelled'
};

// Status Badge Classes
const TourStatusBadgeClasses = {
    'draft': 'bg-secondary',
    'public': 'bg-success',
    'hide': 'bg-warning',
    'cancelled': 'bg-danger'
};

// Status Transition Rules
const TourStatusTransitions = {
    'draft': ['public', 'hide', 'cancelled'],  // Draft can go to all statuses
    'public': ['hide'],                         // Public can only go to Hide
    'hide': ['public'],                         // Hide can only go to Public
    'cancelled': []                             // Cancelled is terminal, no transitions
};

// Authentication Functions
function getAuthToken() {
    return localStorage.getItem('jwt_token');
}

function getAuthHeaders() {
    const token = getAuthToken();
    if (token) {
        return {
            'Authorization': 'Bearer ' + token,
            'Content-Type': 'application/json'
        };
    }
    return {
        'Content-Type': 'application/json'
    };
}

function handleAuthError(response) {
    if (response.status === 401) {
        localStorage.removeItem('jwt_token');
        localStorage.removeItem('refresh_token');
        window.location.href = '/Home/Login';
        return true;
    }
    return false;
}

// Global State
let currentPage = 1;
let currentPageSize = 10;
let currentStatus = '';
let currentSortBy = 'createdtime';
let currentSortOrder = 'desc';
let currentSearch = '';
let selectedTours = new Set();
let draggedTourId = null;
let draggedTourStatus = null;

// Helper Functions
function getStatusDisplayName(status) {
    return TourStatusDisplayNames[status] || 'Unknown';
}

function getStatusBadgeClass(status) {
    return TourStatusBadgeClasses[status] || 'bg-secondary';
}

function canTransitionTo(fromStatus, toStatus) {
    return TourStatusTransitions[fromStatus]?.includes(toStatus) || false;
}

// API Functions
async function fetchTours(status = '', page = 1, pageSize = 10, sortBy = 'createdtime', sortOrder = 'desc', search = '') {
    try {
        const params = new URLSearchParams({
            page: page,
            pageSize: pageSize,
            sortBy: sortBy,
            sortOrder: sortOrder
        });
        
        if (status) params.append('status', status);
        if (search) params.append('search', search);
        
        const response = await fetch(`/api/tour/approval/paged?${params}`, {
            method: 'GET',
            headers: getAuthHeaders()
        });
        
        if (!response.ok) {
            if (handleAuthError(response)) return null;
            throw new Error('Failed to fetch tours');
        }
        
        return await response.json();
    } catch (error) {
        console.error('Error fetching tours:', error);
        showToast('Error loading tours', 'error');
        return null;
    }
}

async function changeTourStatus(tourId, newStatus, reason = '') {
    try {
        const response = await fetch(`/api/tour/approval/${tourId}/status`, {
            method: 'PUT',
            headers: getAuthHeaders(),
            body: JSON.stringify({ newStatus, reason })
        });
        
        if (!response.ok) {
            if (handleAuthError(response)) return false;
        }
        
        const result = await response.json();
        if (result.success) {
            showToast('Status changed successfully', 'success');
            await loadTours();
            return true;
        } else {
            showToast(result.message || 'Failed to change status', 'error');
            return false;
        }
    } catch (error) {
        console.error('Error changing status:', error);
        showToast('Error changing status', 'error');
        return false;
    }
}

async function batchChangeStatus(newStatus) {
    if (selectedTours.size === 0) {
        showToast('Please select tours to change status', 'warning');
        return;
    }
    
    const tourIds = Array.from(selectedTours);
    const reason = prompt(`Reason for changing ${tourIds.length} tour(s) to ${getStatusDisplayName(newStatus)}:`);
    
    if (reason === null) return; // User cancelled
    
    try {
        showLoading(true);
        const response = await fetch('/api/tour/approval/batch-status', {
            method: 'PUT',
            headers: getAuthHeaders(),
            body: JSON.stringify({ tourIds, newStatus, reason })
        });
        
        if (!response.ok) {
            if (handleAuthError(response)) return;
        }
        
        const result = await response.json();
        if (result.success) {
            showToast(result.message, 'success');
            selectedTours.clear();
            await loadTours();
        } else {
            showToast(result.message || 'Failed to change status', 'error');
        }
    } catch (error) {
        console.error('Error in batch status change:', error);
        showToast('Error changing status', 'error');
    } finally {
        showLoading(false);
    }
}

async function batchDelete() {
    if (selectedTours.size === 0) {
        showToast('Please select tours to delete', 'warning');
        return;
    }
    
    const tourIds = Array.from(selectedTours);
    if (!confirm(`Are you sure you want to delete ${tourIds.length} tour(s)? This action cannot be undone.`)) {
        return;
    }
    
    try {
        showLoading(true);
        const response = await fetch('/api/tour/approval/batch-delete', {
            method: 'DELETE',
            headers: getAuthHeaders(),
            body: JSON.stringify({ tourIds })
        });
        
        if (!response.ok) {
            if (handleAuthError(response)) return;
        }
        
        const result = await response.json();
        if (result.success) {
            showToast(result.message, 'success');
            selectedTours.clear();
            await loadTours();
        } else {
            showToast(result.message || 'Failed to delete tours', 'error');
        }
    } catch (error) {
        console.error('Error in batch delete:', error);
        showToast('Error deleting tours', 'error');
    } finally {
        showLoading(false);
    }
}

async function deleteTour(tourId) {
    if (!confirm('Are you sure you want to delete this tour? This action cannot be undone.')) {
        return;
    }
    
    try {
        const response = await fetch('/api/tour/approval/batch-delete', {
            method: 'DELETE',
            headers: getAuthHeaders(),
            body: JSON.stringify({ tourIds: [tourId] })
        });
        
        if (!response.ok) {
            if (handleAuthError(response)) return;
        }
        
        const result = await response.json();
        if (result.success) {
            showToast('Tour deleted successfully', 'success');
            await loadTours();
        } else {
            showToast(result.message || 'Failed to delete tour', 'error');
        }
    } catch (error) {
        console.error('Error deleting tour:', error);
        showToast('Error deleting tour', 'error');
    }
}

async function viewTourDetail(tourId) {
    try {
        const response = await fetch(`/api/tour/${tourId}`, {
            method: 'GET',
            headers: getAuthHeaders()
        });
        
        if (!response.ok) {
            if (handleAuthError(response)) return;
            throw new Error('Failed to fetch tour details');
        }
        
        const result = await response.json();
        
        if (result.success && result.data) {
            const tour = result.data;
            const content = `
                <div class="row">
                    <div class="col-md-6">
                        <p><strong>Title:</strong> ${tour.title}</p>
                        <p><strong>Status:</strong> <span class="badge ${getStatusBadgeClass(tour.status)}">${getStatusDisplayName(tour.status)}</span></p>
                        <p><strong>Price:</strong> <span class="price-display">${formatCurrency(tour.pricePerPerson)}</span></p>
                        <p><strong>Duration:</strong> ${tour.duration}</p>
                        <p><strong>Max People:</strong> ${tour.maxPeople}</p>
                    </div>
                    <div class="col-md-6">
                        <p><strong>Created:</strong> ${new Date(tour.createdTime).toLocaleString()}</p>
                        <p><strong>Updated:</strong> ${tour.updatedTime ? new Date(tour.updatedTime).toLocaleString() : 'N/A'}</p>
                        <p><strong>Updated By:</strong> ${tour.updatedBy || 'N/A'}</p>
                    </div>
                    <div class="col-12 mt-3">
                        <p><strong>Description:</strong></p>
                        <p>${tour.description || 'No description'}</p>
                    </div>
                </div>
            `;
            
            document.getElementById('tourDetailContent').innerHTML = content;
            const modal = new bootstrap.Modal(document.getElementById('tourDetailModal'));
            modal.show();
        } else {
            showToast('Failed to load tour details', 'error');
        }
    } catch (error) {
        console.error('Error loading tour details:', error);
        showToast('Error loading tour details', 'error');
    }
}

async function viewStatusHistory(tourId) {
    try {
        const response = await fetch(`/api/tour/approval/${tourId}/history`, {
            method: 'GET',
            headers: getAuthHeaders()
        });
        
        if (!response.ok) {
            if (handleAuthError(response)) return;
            throw new Error('Failed to fetch status history');
        }
        
        const result = await response.json();
        
        if (result.success && result.data) {
            const history = result.data;
            let content = '<div class="timeline">';
            
            if (history.length === 0) {
                content = '<p class="text-muted">No status change history</p>';
            } else {
                history.forEach(item => {
                    content += `
                        <div class="mb-3 pb-3 border-bottom">
                            <div class="d-flex justify-content-between align-items-start">
                                <div>
                                    <strong>${item.oldStatusDisplayName || 'Initial'}</strong> 
                                    <i class="ti ti-arrow-right mx-2"></i>
                                    <strong class="${getStatusBadgeClass(item.newStatus)}">${item.newStatusDisplayName}</strong>
                                </div>
                                <small class="text-muted">${new Date(item.changedTime).toLocaleString()}</small>
                            </div>
                            <small class="text-muted">Changed by: ${item.changedBy}</small>
                            ${item.reason ? `<p class="mb-0 mt-2"><em>"${item.reason}"</em></p>` : ''}
                        </div>
                    `;
                });
            }
            content += '</div>';
            
            document.getElementById('statusHistoryContent').innerHTML = content;
            const modal = new bootstrap.Modal(document.getElementById('statusHistoryModal'));
            modal.show();
        } else {
            showToast('Failed to load status history', 'error');
        }
    } catch (error) {
        console.error('Error loading status history:', error);
        showToast('Error loading status history', 'error');
    }
}

// Rendering Functions
function renderTableView(tours) {
    const tbody = document.getElementById('toursTableBody');
    tbody.innerHTML = '';
    
    if (tours.length === 0) {
        showEmptyState(true);
        return;
    }
    
    showEmptyState(false);
    
    tours.forEach(tour => {
        const row = document.createElement('tr');
        row.className = 'tour-row';
        row.dataset.tourId = tour.id;
        row.dataset.status = tour.status;
        row.draggable = true;
        
        const imageUrl = tour.bannerImageUrl || tour.images[0] || '/images/no-image.png';
        const categories = tour.categories.map(c => c.categoryName).join(', ') || 'N/A';
        
        row.innerHTML = `
            <td>
                <input type="checkbox" class="form-check-input tour-checkbox" 
                       value="${tour.id}" data-status="${tour.status}"
                       ${selectedTours.has(tour.id) ? 'checked' : ''}>
            </td>
            <td>
                <img src="${imageUrl}" class="rounded" 
                     style="width: 60px; height: 60px; object-fit: cover;" alt="${tour.title}">
            </td>
            <td>
                <h6 class="mb-0">${tour.title}</h6>
                <small class="text-muted">${categories}</small>
            </td>
            <td>
                <span class="badge ${getStatusBadgeClass(tour.status)}">${tour.statusDisplayName}</span>
            </td>
            <td>${formatCurrency(tour.pricePerPerson)}</td>
            <td>${tour.duration}</td>
            <td><small class="text-muted">${new Date(tour.createdTime).toLocaleDateString()}</small></td>
            <td class="text-end">
                <button class="btn btn-sm btn-light-primary" data-bs-toggle="tooltip" title="View Details" 
                        onclick="viewTourDetail(${tour.id})">
                    <i class="ti ti-eye"></i>
                </button>
                <button class="btn btn-sm btn-light-info" data-bs-toggle="tooltip" title="Status History" 
                        onclick="viewStatusHistory(${tour.id})">
                    <i class="ti ti-history"></i>
                </button>
                <button class="btn btn-sm btn-light-danger" data-bs-toggle="tooltip" title="Delete" 
                        onclick="deleteTour(${tour.id})">
                    <i class="ti ti-trash"></i>
                </button>
            </td>
        `;
        
        tbody.appendChild(row);
        attachDragListeners(row);
    });
    
    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

function renderCardView(tours) {
    const container = document.getElementById('cardView');
    container.innerHTML = '';
    
    if (tours.length === 0) {
        showEmptyState(true);
        return;
    }
    
    showEmptyState(false);
    
    tours.forEach(tour => {
        const imageUrl = tour.bannerImageUrl || tour.images[0] || '/images/no-image.png';
        const categories = tour.categories.map(c => c.categoryName).join(', ') || 'N/A';
        
        const col = document.createElement('div');
        col.className = 'col-lg-3 col-md-4 col-sm-6 mb-4';
        
        col.innerHTML = `
            <div class="card tour-card h-100 shadow-sm" data-tour-id="${tour.id}" data-status="${tour.status}" draggable="true">
                <div class="position-relative">
                    <img src="${imageUrl}" class="card-img-top" style="height: 200px; object-fit: cover;" alt="${tour.title}">
                    <div class="position-absolute top-0 start-0 p-2">
                        <input type="checkbox" class="form-check-input tour-checkbox bg-white" 
                               value="${tour.id}" data-status="${tour.status}"
                               ${selectedTours.has(tour.id) ? 'checked' : ''}>
                    </div>
                    <div class="position-absolute top-0 end-0 p-2">
                        <span class="badge ${getStatusBadgeClass(tour.status)}">${tour.statusDisplayName}</span>
                    </div>
                </div>
                <div class="card-body">
                    <h6 class="card-title">${tour.title}</h6>
                    <p class="card-text text-muted small">${categories}</p>
                    <div class="d-flex justify-content-between align-items-center mt-3">
                        <div>
                            <small class="text-muted">Price:</small>
                            <h6 class="mb-0">${formatCurrency(tour.pricePerPerson)}</h6>
                        </div>
                        <div>
                            <small class="text-muted">${tour.duration}</small>
                        </div>
                    </div>
                </div>
                <div class="card-footer bg-transparent border-top-0">
                    <div class="d-flex justify-content-end gap-2">
                        <button class="btn btn-sm btn-light-primary" data-bs-toggle="tooltip" title="View Details" 
                                onclick="viewTourDetail(${tour.id})">
                            <i class="ti ti-eye"></i>
                        </button>
                        <button class="btn btn-sm btn-light-info" data-bs-toggle="tooltip" title="Status History" 
                                onclick="viewStatusHistory(${tour.id})">
                            <i class="ti ti-history"></i>
                        </button>
                        <button class="btn btn-sm btn-light-danger" data-bs-toggle="tooltip" title="Delete" 
                                onclick="deleteTour(${tour.id})">
                            <i class="ti ti-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        container.appendChild(col);
        const card = col.querySelector('.tour-card');
        attachDragListeners(card);
    });
    
    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

function renderPagination(totalPages, currentPageNum) {
    const pagination = document.getElementById('pagination');
    pagination.innerHTML = '';
    
    if (totalPages <= 1) {
        document.getElementById('paginationContainer').style.display = 'none';
        return;
    }
    
    document.getElementById('paginationContainer').style.display = 'block';
    
    // Previous button
    const prevLi = document.createElement('li');
    prevLi.className = `page-item ${currentPageNum === 1 ? 'disabled' : ''}`;
    prevLi.innerHTML = `<a class="page-link" href="#" data-page="${currentPageNum - 1}">Previous</a>`;
    pagination.appendChild(prevLi);
    
    // Page numbers
    const startPage = Math.max(1, currentPageNum - 2);
    const endPage = Math.min(totalPages, currentPageNum + 2);
    
    for (let i = startPage; i <= endPage; i++) {
        const li = document.createElement('li');
        li.className = `page-item ${i === currentPageNum ? 'active' : ''}`;
        li.innerHTML = `<a class="page-link" href="#" data-page="${i}">${i}</a>`;
        pagination.appendChild(li);
    }
    
    // Next button
    const nextLi = document.createElement('li');
    nextLi.className = `page-item ${currentPageNum === totalPages ? 'disabled' : ''}`;
    nextLi.innerHTML = `<a class="page-link" href="#" data-page="${currentPageNum + 1}">Next</a>`;
    pagination.appendChild(nextLi);
}

// Drag and Drop
function attachDragListeners(element) {
    element.addEventListener('dragstart', handleDragStart);
    element.addEventListener('dragend', handleDragEnd);
}

function handleDragStart(e) {
    const tourId = this.dataset.tourId;
    const tourStatus = this.dataset.status;
    
    draggedTourId = tourId;
    draggedTourStatus = tourStatus;
    
    this.classList.add('dragging');
    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/html', this.innerHTML);
}

function handleDragEnd(e) {
    this.classList.remove('dragging');
    draggedTourId = null;
    draggedTourStatus = null;
}

function initializeWorkflowZones() {
    const zones = document.querySelectorAll('.workflow-zone');
    
    zones.forEach(zone => {
        zone.addEventListener('dragover', handleDragOver);
        zone.addEventListener('drop', handleDrop);
        zone.addEventListener('dragleave', handleDragLeave);
    });
}

function handleDragOver(e) {
    if (e.preventDefault) {
        e.preventDefault();
    }
    
    e.dataTransfer.dropEffect = 'move';
    this.classList.add('drag-over');
    this.querySelector('.drop-indicator').style.display = 'block';
    
    return false;
}

function handleDragLeave(e) {
    this.classList.remove('drag-over');
    this.querySelector('.drop-indicator').style.display = 'none';
}

async function handleDrop(e) {
    if (e.stopPropagation) {
        e.stopPropagation();
    }
    
    this.classList.remove('drag-over');
    this.querySelector('.drop-indicator').style.display = 'none';
    
    const targetStatus = this.dataset.targetStatus;
    
    if (!draggedTourId || !draggedTourStatus) return false;
    
    if (draggedTourStatus === targetStatus) {
        showToast('Tour is already in this status', 'info');
        return false;
    }
    
    if (!canTransitionTo(draggedTourStatus, targetStatus)) {
        showToast(`Cannot transition from ${getStatusDisplayName(draggedTourStatus)} to ${getStatusDisplayName(targetStatus)}`, 'error');
        return false;
    }
    
    await changeTourStatus(parseInt(draggedTourId), targetStatus, 'Changed via drag and drop');
    
    return false;
}

// Selection Management
function handleCheckboxChange(e) {
    const tourId = parseInt(e.target.value);
    const tourStatus = e.target.dataset.status;
    
    if (e.target.checked) {
        // If this is the first selection or same status, add it
        if (selectedTours.size === 0) {
            selectedTours.add(tourId);
        } else {
            // Check if all selected tours have the same status
            const firstSelectedCheckbox = document.querySelector('.tour-checkbox:checked');
            if (firstSelectedCheckbox && firstSelectedCheckbox.dataset.status === tourStatus) {
                selectedTours.add(tourId);
            } else {
                e.target.checked = false;
                showToast('You can only select tours with the same status', 'warning');
            }
        }
    } else {
        selectedTours.delete(tourId);
    }
    
    updateBatchActionsUI();
}

function handleSelectAll(e) {
    const checkboxes = document.querySelectorAll('.tour-checkbox');
    
    if (e.target.checked) {
        // Get first tour's status
        const firstCheckbox = checkboxes[0];
        if (!firstCheckbox) return;
        
        const targetStatus = firstCheckbox.dataset.status;
        
        checkboxes.forEach(cb => {
            if (cb.dataset.status === targetStatus) {
                cb.checked = true;
                selectedTours.add(parseInt(cb.value));
            }
        });
    } else {
        checkboxes.forEach(cb => cb.checked = false);
        selectedTours.clear();
    }
    
    updateBatchActionsUI();
}

function updateBatchActionsUI() {
    const section = document.getElementById('batchActionsSection');
    const count = document.getElementById('selectedCount');
    const statusInfo = document.getElementById('selectedStatusInfo');
    
    count.textContent = selectedTours.size;
    
    if (selectedTours.size === 0) {
        section.style.display = 'none';
        return;
    }
    
    section.style.display = 'block';
    
    // Get the status of selected tours
    const firstSelectedCheckbox = document.querySelector('.tour-checkbox:checked');
    const currentStatus = firstSelectedCheckbox ? firstSelectedCheckbox.dataset.status : '';
    
    // Update status info
    statusInfo.textContent = `(${getStatusDisplayName(currentStatus)} tours)`;
    
    // Enable/disable buttons based on current status and allowed transitions
    const btnPublic = document.getElementById('btnBatchPublic');
    const btnHide = document.getElementById('btnBatchHide');
    const btnCancelled = document.getElementById('btnBatchCancelled');
    
    // Reset all buttons
    [btnPublic, btnHide, btnCancelled].forEach(btn => {
        btn.disabled = false;
        btn.classList.remove('disabled');
    });
    
    // Apply transition rules
    if (currentStatus) {
        const allowedTransitions = TourStatusTransitions[currentStatus] || [];
        
        // Disable buttons for transitions that are not allowed
        if (!allowedTransitions.includes('public')) {
            btnPublic.disabled = true;
            btnPublic.classList.add('disabled');
            btnPublic.title = `Cannot change ${getStatusDisplayName(currentStatus)} tours to Public`;
        }
        
        if (!allowedTransitions.includes('hide')) {
            btnHide.disabled = true;
            btnHide.classList.add('disabled');
            btnHide.title = `Cannot change ${getStatusDisplayName(currentStatus)} tours to Hide`;
        }
        
        if (!allowedTransitions.includes('cancelled')) {
            btnCancelled.disabled = true;
            btnCancelled.classList.add('disabled');
            btnCancelled.title = `Cannot change ${getStatusDisplayName(currentStatus)} tours to Cancelled`;
        }
    }
}

// Load Tours
async function loadTours() {
    showLoading(true);
    
    const result = await fetchTours(currentStatus, currentPage, currentPageSize, currentSortBy, currentSortOrder, currentSearch);
    
    showLoading(false);
    
    if (!result) return;
    
    // Update status badges
    updateStatusBadges(result.totalItems, currentStatus);
    
    // Render tours
    const isTableView = document.getElementById('viewTable').classList.contains('active');
    if (isTableView) {
        renderTableView(result.items);
    } else {
        renderCardView(result.items);
    }
    
    // Render pagination
    renderPagination(result.totalPages, result.currentPage);
    
    // Attach checkbox listeners
    attachCheckboxListeners();
}

function attachCheckboxListeners() {
    document.querySelectorAll('.tour-checkbox').forEach(cb => {
        cb.addEventListener('change', handleCheckboxChange);
    });
    
    const selectAll = document.getElementById('selectAll');
    if (selectAll) {
        selectAll.addEventListener('change', handleSelectAll);
    }
}

async function updateStatusBadges(totalCount, activeStatus) {
    // This would ideally come from the API with counts per status
    // For now, we'll just show the total for the active filter
    document.getElementById('badge-all').textContent = activeStatus === '' ? totalCount : '...';
    document.getElementById('badge-draft').textContent = activeStatus === 'draft' ? totalCount : '...';
    document.getElementById('badge-public').textContent = activeStatus === 'public' ? totalCount : '...';
    document.getElementById('badge-hide').textContent = activeStatus === 'hide' ? totalCount : '...';
    document.getElementById('badge-cancelled').textContent = activeStatus === 'cancelled' ? totalCount : '...';
}

// UI Helper Functions
function showLoading(show) {
    document.getElementById('loadingIndicator').style.display = show ? 'block' : 'none';
}

function showEmptyState(show) {
    document.getElementById('emptyState').style.display = show ? 'block' : 'none';
}

// Wrapper for showToast() from toast.js
// Auto-maps 'error' → 'danger' for compatibility
const originalShowToast = window.showToast;
window.showToast = function(message, type = 'info') {
    // Map 'error' to 'danger' for toast.js compatibility
    const toastType = type === 'error' ? 'danger' : type;
    
    // Use the original showToast function from toast.js
    if (typeof originalShowToast === 'function') {
        return originalShowToast(message, toastType);
    } else {
        console.log(`Toast: ${message} (${type})`);
        alert(message);
    }
};

// Event Handlers
document.addEventListener('DOMContentLoaded', function() {
    // Load initial tours
    loadTours();
    
    // Initialize workflow zones
    initializeWorkflowZones();
    
    // Status tab click
    document.querySelectorAll('#statusTabs .nav-link').forEach(tab => {
        tab.addEventListener('click', function() {
            document.querySelectorAll('#statusTabs .nav-link').forEach(t => t.classList.remove('active'));
            this.classList.add('active');
            
            currentStatus = this.dataset.status || '';
            currentPage = 1;
            selectedTours.clear();
            loadTours();
        });
    });
    
    // View toggle
    document.getElementById('viewTable').addEventListener('click', function() {
        document.getElementById('viewTable').classList.add('active');
        document.getElementById('viewCards').classList.remove('active');
        document.getElementById('tableView').style.display = 'block';
        document.getElementById('cardView').style.display = 'none';
        loadTours();
    });
    
    document.getElementById('viewCards').addEventListener('click', function() {
        document.getElementById('viewCards').classList.add('active');
        document.getElementById('viewTable').classList.remove('active');
        document.getElementById('tableView').style.display = 'none';
        document.getElementById('cardView').style.display = 'flex';
        loadTours();
    });
    
    // Search
    let searchTimeout;
    document.getElementById('searchInput').addEventListener('input', function(e) {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            currentSearch = e.target.value;
            currentPage = 1;
            loadTours();
        }, 500);
    });
    
    document.getElementById('btnClearSearch').addEventListener('click', function() {
        document.getElementById('searchInput').value = '';
        currentSearch = '';
        currentPage = 1;
        loadTours();
    });
    
    // Pagination
    document.getElementById('pagination').addEventListener('click', function(e) {
        if (e.target.tagName === 'A') {
            e.preventDefault();
            const page = parseInt(e.target.dataset.page);
            if (page && page !== currentPage) {
                currentPage = page;
                loadTours();
            }
        }
    });
});

