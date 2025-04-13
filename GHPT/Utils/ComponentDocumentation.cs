using System;

namespace GHPT.Utils
{
    public static class ComponentDocumentation
    {
        public const string Documentation = @"# Grasshopper Component Documentation

## Geometry Components

### Points
#### Construct Point
- Description: Creates a point from coordinates
- Inputs:
  - Point: {x,y,z} coordinates
- Outputs:
  - Point: The constructed point
- Validation Rules:
  - Coordinates must be valid numbers
  - Output can connect to any point input

#### Point List
- Description: Creates a list of points
- Inputs:
  - Points: Multiple points to combine
- Outputs:
  - Points: Combined list of points
- Validation Rules:
  - All inputs must be points
  - Output can connect to any point list input

#### Point XYZ
- Description: Creates a point directly from X, Y, Z coordinates
- Inputs:
  - X: X coordinate
  - Y: Y coordinate
  - Z: Z coordinate
- Outputs:
  - Point: The constructed point
- Validation Rules:
  - All coordinates must be numbers
  - Output can connect to any point input

### Curves
#### Interpolate
- Description: Creates a smooth curve that passes through all points
- Inputs:
  - Points: List of points to interpolate through
  - Degree: Degree of the curve (default: 3)
- Outputs:
  - Curve: Interpolated curve
- Validation Rules:
  - Requires at least 2 points
  - Points must be in desired order
  - Output can connect to any curve input

#### Nurbs Curve
- Description: Creates a NURBS curve through points
- Inputs:
  - Points: Control points for the curve
  - Degree: Degree of the curve
  - Knots: Knot vector (optional)
- Outputs:
  - Curve: NURBS curve
- Validation Rules:
  - Requires at least degree+1 points
  - Output can connect to any curve input

#### Polyline
- Description: Creates a polyline connecting points in order
- Inputs:
  - Points: Points to connect
- Outputs:
  - Curve: Polyline curve
- Validation Rules:
  - Requires at least 2 points
  - Points must be in desired order
  - Output can connect to any curve input

#### Circle CNR
- Description: Creates a circle from center, normal, and radius
- Inputs:
  - Center: Point for circle center
  - Normal: Vector for circle normal
  - Radius: Circle radius
- Outputs:
  - Circle: Resulting circle
- Validation Rules:
  - Center must be a point
  - Normal must be a vector
  - Radius must be a number
  - Output can only connect to curve inputs

#### Line
- Description: Creates a line between two points
- Inputs:
  - Start Point: Starting point of line
  - End Point: Ending point of line
- Outputs:
  - Line: Resulting line
- Validation Rules:
  - Both points must be valid
  - Output can only connect to curve inputs

### Surfaces
#### Loft
- Description: Creates a surface between curves
- Inputs:
  - Curves: Multiple curves to loft between
- Outputs:
  - Surface: Resulting lofted surface
- Validation Rules:
  - Requires at least 2 curves as input
  - All curves must be in the same plane
  - Curves must be compatible for lofting
  - Output can only connect to surface inputs

#### Extrude
- Description: Extrudes a curve along a direction
- Inputs:
  - Base: Curve to extrude
  - Direction: Vector for extrusion direction
- Outputs:
  - Surface: Resulting extruded surface
- Validation Rules:
  - Base must be a curve
  - Direction must be a vector
  - Output can only connect to surface inputs

## Transform Components

### Move
- Description: Moves geometry along a vector
- Inputs:
  - Geometry: Geometry to move
  - Motion: Vector for movement direction
  - Distance: Distance to move
- Outputs:
  - Geometry: Moved geometry
- Validation Rules:
  - Geometry must be valid
  - Motion must be a vector
  - Distance must be a number

### Rotate
- Description: Rotates geometry around an axis
- Inputs:
  - Geometry: Geometry to rotate
  - Axis: Line to rotate around
  - Angle: Angle in degrees
- Outputs:
  - Geometry: Rotated geometry
- Validation Rules:
  - Geometry must be valid
  - Axis must be a line
  - Angle must be a number

### Scale
- Description: Scales geometry uniformly or non-uniformly
- Inputs:
  - Geometry: Geometry to scale
  - Center: Center point of scaling
  - Factor: Scale factor
- Outputs:
  - Geometry: Scaled geometry
- Validation Rules:
  - Geometry must be valid
  - Center must be a point
  - Factor must be a number

## Data Components

### Series
- Description: Creates a series of numbers
- Inputs:
  - Start: Starting value
  - Step: Step size
  - Count: Number of values
- Outputs:
  - Series: List of numbers
- Validation Rules:
  - All inputs must be numbers
  - Count must be positive
  - Output can connect to any numeric input

### Cross Reference
- Description: Creates combinations of two lists
- Inputs:
  - A: First list
  - B: Second list
- Outputs:
  - Result: All combinations of A and B
- Validation Rules:
  - Both inputs must be lists
  - Output can connect to any list input

### Number Slider
- Description: Creates a numeric input with min/max/current values
- Inputs: None
- Outputs:
  - number: The current value of the slider
- Validation Rules:
  - Output can only connect to numeric inputs
  - Value must be within min/max range

## Boolean Components

### Addition
- Description: Adds two numbers together
- Inputs:
  - A: First number to add
  - B: Second number to add
- Outputs:
  - Result: Sum of A and B
- Validation Rules:
  - Both inputs must be numeric
  - Output can only connect to numeric inputs

### Multiplication
- Description: Multiplies two numbers
- Inputs:
  - A: First number
  - B: Second number
- Outputs:
  - Result: Product of A and B
- Validation Rules:
  - Both inputs must be numeric
  - Output can only connect to numeric inputs

### Division
- Description: Divides two numbers
- Inputs:
  - A: First number
  - B: Second number
- Outputs:
  - Result: Quotient of A and B
- Validation Rules:
  - Both inputs must be numeric
  - B cannot be zero
  - Output can only connect to numeric inputs";

        public static string GetDocumentation()
        {
            return Documentation;
        }
    }
} 