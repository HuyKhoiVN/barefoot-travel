# Toast Message Styles

This document describes the CSS styles for toast messages, categorized by their `Property 2` values: Default, Info, Success, and Warning. Each style shares common properties, with specific differences in background color, border color, and order.

## Common Properties
All toast message styles share the following properties:

- **Box Sizing**: `box-sizing: border-box;`
- **Layout**:
  - `display: flex;`
  - `flex-direction: row;`
  - `align-items: center;`
  - `padding: 12px 16px;`
  - `gap: 12px;`
- **Dimensions**:
  - `width: 350px;`
  - `height: 56px;`
- **Shadow**: `box-shadow: 0px 16px 20px -8px rgba(3, 5, 18, 0.1);`
- **Border Radius**: `border-radius: 16px;`
- **Auto Layout**:
  - `flex: none;`
  - `flex-grow: 0;`

## Style Variants

### 1. Default
- **Background**: `#FFFFFF` (White)
- **Border**: `1px solid #FBFBFB`
- **Order**: `10`

### 2. Info
- **Background**: `#EDF2FD` (Light Blue)
- **Border**: `1px solid #4B85F5` (Blue)
- **Order**: `11`

### 3. Success
- **Background**: `#E5FCF1` (Light Green)
- **Border**: `1px solid #01E17B` (Green)
- **Order**: `12`
- **Note**: The Success style appears twice in the provided file with identical properties.

### 4. Warning
- **Background**: `#FFFAE8` (Light Yellow)
- **Border**: `1px solid #FDCD0F` (Yellow)
- **Order**: `14`

## Notes
- The `Success` style is duplicated in the input file with no differences in properties.
- The `order` property varies across styles, likely used for controlling the stacking or display order in a flex container.
- These styles are designed for a consistent toast message appearance with variations in color to indicate different message types (e.g., informational, success, warning).