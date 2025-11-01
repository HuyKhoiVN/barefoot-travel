-- ============================================
-- POPULATE SLUGS FOR EXISTING CATEGORIES
-- Run this to auto-generate slugs for categories without slug
-- ============================================

-- Step 1: Show categories that need slugs
SELECT 
    Id,
    CategoryName,
    Slug,
    ShowInWaysToTravel,
    ShowInDailyTours,
    Active
FROM Category 
WHERE (ShowInWaysToTravel = 1 OR ShowInDailyTours = 1)
  AND Active = 1
ORDER BY WaysToTravelOrder, DailyTourOrder;

-- Step 2: Auto-generate slugs
UPDATE Category
SET Slug = LOWER(
    -- Replace Vietnamese characters
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(
        -- Replace separators
        REPLACE(
        REPLACE(
        REPLACE(
        REPLACE(
            CategoryName,
            ' - ', '-'
        ),
            ' / ', '-'
        ),
            ' & ', '-'
        ),
            ' + ', '-'
        ),
        -- Vietnamese characters
        N'Đ', 'd'
    ),
        N'đ', 'd'
    ),
        N'À', 'a'
    ),
        N'Á', 'a'
    ),
        N'à', 'a'
    ),
        N'á', 'a'
    ),
        N'Ô', 'o'
    ),
        N'ô', 'o'
    ),
        N'Ư', 'u'
    ),
        N'ư', 'u'
    )
)
WHERE Slug IS NULL;

-- Step 3: Clean up (remove spaces, special chars)
UPDATE Category
SET Slug = REPLACE(REPLACE(REPLACE(Slug, ' ', '-'), '(', ''), ')', '')
WHERE Slug IS NOT NULL;

-- Step 4: Handle duplicates (add suffix)
WITH DuplicateSlugs AS (
    SELECT Slug, COUNT(*) as Count
    FROM Category
    WHERE Slug IS NOT NULL
    GROUP BY Slug
    HAVING COUNT(*) > 1
)
UPDATE c
SET c.Slug = c.Slug + '-' + CAST(c.Id AS NVARCHAR(10))
FROM Category c
INNER JOIN DuplicateSlugs d ON c.Slug = d.Slug
WHERE c.Id NOT IN (
    SELECT MIN(Id) 
    FROM Category 
    GROUP BY Slug
);

-- Step 5: Verify slugs are populated
SELECT 
    Id,
    CategoryName,
    Slug,
    ShowInWaysToTravel,
    ShowInDailyTours
FROM Category 
WHERE (ShowInWaysToTravel = 1 OR ShowInDailyTours = 1)
  AND Active = 1
ORDER BY WaysToTravelOrder, DailyTourOrder;

-- Expected: All rows should have Slug value (not NULL)

-- ============================================
-- OPTIONAL: Populate slugs for ALL categories
-- ============================================

-- If you want slugs for all categories (not just homepage ones):
UPDATE Category
SET Slug = LOWER(
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(
        REPLACE(
        REPLACE(
        REPLACE(
            CategoryName,
            ' - ', '-'
        ),
            ' / ', '-'
        ),
            ' & ', '-'
        ),
            ' + ', '-'
        ),
        N'Đ', 'd'
    ),
        N'đ', 'd'
    ),
        ' ', '-'
    )
)
WHERE Slug IS NULL;

-- Clean up
UPDATE Category
SET Slug = REPLACE(REPLACE(REPLACE(Slug, '(', ''), ')', ''), '--', '-')
WHERE Slug IS NOT NULL;

-- Handle duplicates
WITH DuplicateSlugs AS (
    SELECT Slug, COUNT(*) as Count
    FROM Category
    WHERE Slug IS NOT NULL
    GROUP BY Slug
    HAVING COUNT(*) > 1
)
UPDATE c
SET c.Slug = c.Slug + '-' + CAST(c.Id AS NVARCHAR(10))
FROM Category c
INNER JOIN DuplicateSlugs d ON c.Slug = d.Slug
WHERE c.Id NOT IN (
    SELECT MIN(Id) 
    FROM Category 
    GROUP BY Slug
);

-- Verify ALL categories
SELECT Id, CategoryName, Slug 
FROM Category 
WHERE Active = 1
ORDER BY Type, CategoryName;

-- ============================================
-- POPULATE SLUGS FOR TOURS
-- ============================================

-- Show tours without slugs
SELECT Id, Title, Slug, Active
FROM Tour
WHERE Active = 1 AND Slug IS NULL;

-- Auto-generate for tours
UPDATE Tour
SET Slug = LOWER(
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(
    REPLACE(
        REPLACE(
        REPLACE(
            Title,
            ' - ', '-'
        ),
            ' / ', '-'
        ),
            ' & ', '-'
        ),
            ' + ', '-'
        ),
        N'Đ', 'd'
    ),
        N'đ', 'd'
    ),
        ' ', '-'
    )
)
WHERE Slug IS NULL;

-- Clean up tours
UPDATE Tour
SET Slug = REPLACE(REPLACE(REPLACE(Slug, '(', ''), ')', ''), '--', '-')
WHERE Slug IS NOT NULL;

-- Handle tour duplicates
WITH DuplicateSlugs AS (
    SELECT Slug, COUNT(*) as Count
    FROM Tour
    WHERE Slug IS NOT NULL
    GROUP BY Slug
    HAVING COUNT(*) > 1
)
UPDATE t
SET t.Slug = t.Slug + '-' + CAST(t.Id AS NVARCHAR(10))
FROM Tour t
INNER JOIN DuplicateSlugs d ON t.Slug = d.Slug
WHERE t.Id NOT IN (
    SELECT MIN(Id) 
    FROM Tour 
    GROUP BY Slug
);

-- Verify tours
SELECT Id, Title, Slug, Active
FROM Tour
WHERE Active = 1
ORDER BY Title;

-- ============================================
-- VALIDATION QUERIES
-- ============================================

-- Check for NULL slugs (should be 0 or acceptable)
SELECT COUNT(*) as CategoriesWithoutSlug
FROM Category
WHERE Active = 1 AND Slug IS NULL;

SELECT COUNT(*) as ToursWithoutSlug
FROM Tour
WHERE Active = 1 AND Slug IS NULL;

-- Check for duplicate slugs (should be 0)
SELECT Slug, COUNT(*) as Count
FROM Category
WHERE Slug IS NOT NULL
GROUP BY Slug
HAVING COUNT(*) > 1;

SELECT Slug, COUNT(*) as Count
FROM Tour
WHERE Slug IS NOT NULL
GROUP BY Slug
HAVING COUNT(*) > 1;

-- Show final results
SELECT 'Categories with slug' as Type, COUNT(*) as Count
FROM Category WHERE Slug IS NOT NULL
UNION ALL
SELECT 'Tours with slug', COUNT(*)
FROM Tour WHERE Slug IS NOT NULL;




