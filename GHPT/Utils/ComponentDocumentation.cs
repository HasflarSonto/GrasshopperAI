using System;

namespace GHPT.Utils
{
    public static class ComponentDocumentation
    {
        public const string Documentation = @"
# Grasshopper Component Documentation

## Geometry Components

### Point
- Category: Params/Geometry
- Description: Defines a point in 3D space using X, Y, and Z coordinates
- Inputs: None
- Outputs:
  - Point (Point3D): The defined point
- Value Format: {x,y,z}
- Example: {0,0,0}

### Plane
- Category: Params/Geometry
- Description: Defines a plane in 3D space using an origin point and a Z-axis vector
- Inputs: None
- Outputs:
  - Plane (Plane): The defined plane
- Value Format: {origin_x,origin_y,origin_z},{z_x,z_y,z_z}
- Example: {6.07,-14.22,0.00},{-0.32,-0.56,-0.77}
- Notes: 
  - The first set of coordinates defines the origin point of the plane
  - The second set of coordinates defines the Z-axis vector direction
  - The X and Y axes are automatically calculated based on the Z-axis vector

### Line
- Category: Curve/Primitive
- Description: Creates a straight line segment between two points
- Inputs:
  - A (Point3D): Start point of the line
  - B (Point3D): End point of the line
- Outputs:
  - Line (Curve): The resulting line segment
- Value Format: {x1,y1,z1} to {x2,y2,z2}
- Example: {0,0,0} to {1,1,1}

### Circle
- Category: Curve/Primitive
- Description: Creates a circle defined by a plane and radius
- Inputs:
  - P (Plane): Base plane for the circle
  - R (Number): Radius of the circle
- Outputs:
  - Circle (Curve): The resulting circle
- Value Format: Plane: {origin, x-axis, y-axis}, Radius: number
- Example: Plane: {0,0,0,1,0,0,0,1,0}, Radius: 1.0

### Rectangle
- Category: Curve/Primitive
- Description: Creates a rectangle on a given plane
- Inputs:
  - P (Plane): Base plane of the rectangle
  - X (Domain): Length in X direction
  - Y (Domain): Length in Y direction
  - R (Number): Corner radius (optional)
- Outputs:
  - Rectangle (Curve): The resulting rectangle
  - Length (Number): The perimeter length
- Value Format: Plane: {origin, x-axis, y-axis}, X: {min,max}, Y: {min,max}, R: number
- Example: Plane: {0,0,0,1,0,0,0,1,0}, X: {0,1}, Y: {0,1}, R: 0

### Box 2Pt
- Category: Surface/Primitive
- Description: Creates a box defined by two opposite corner points
- Inputs:
  - A (Point3D): First corner point
  - B (Point3D): Opposite corner point
- Outputs:
  - Box (Brep): The resulting box
- Value Format: {x1,y1,z1} to {x2,y2,z2}
- Example: {0,0,0} to {1,1,1}

## Curve Components

### Interpolate
- Category: Curve/Spline
- Description: Creates a curve that passes through a set of points
- Inputs:
  - V (Point3D): List of points to interpolate
  - D (Integer): Degree of the curve
  - P (Boolean): Periodic curve (optional)
  - K (Integer): Knot style (optional)
- Outputs:
  - Curve (Curve): The interpolated curve
  - Length (Number): The curve length
  - Domain (Domain): The parameter domain
- Value Format: Points: [{x1,y1,z1}, {x2,y2,z2}, ...], Degree: integer
- Example: Points: [{0,0,0}, {1,1,0}, {2,0,0}], Degree: 3

## Surface Components

### Surface From Points
- Category: Surface/Freeform
- Description: Creates a surface from a grid of points
- Inputs:
  - P (Point3D): Grid of points
  - U (Integer): Number of points in U direction
  - I (Boolean): Interpolate points (optional)
- Outputs:
  - Surface (Surface): The resulting surface
- Value Format: Points: [{x1,y1,z1}, {x2,y2,z2}, ...], U: integer
- Example: Points: [{0,0,0}, {1,0,0}, {0,1,0}, {1,1,0}], U: 2

### Loft
- Category: Surface/Freeform
- Description: Creates a surface through a series of curves
- Inputs:
  - C (Curve): List of section curves
  - O (Loft Options): Loft options (optional)
- Outputs:
  - Loft (Brep): The resulting surface
- Value Format: Curves: [curve1, curve2, ...]
- Example: Curves: [circle1, circle2, circle3]

## Solid Operations

### Solid Union
- Category: Intersect/Boolean
- Description: Combines multiple solids into one
- Inputs:
  - B (Brep): List of solids to union
- Outputs:
  - Result (Brep): The combined solid
- Value Format: Breps: [brep1, brep2, ...]
- Example: Breps: [box1, box2]

### Solid Difference
- Category: Intersect/Boolean
- Description: Subtracts one solid from another
- Inputs:
  - A (Brep): Base solid
  - B (Brep): Solid to subtract
- Outputs:
  - Result (Brep): The resulting solid
- Value Format: Base: brep1, Subtract: brep2
- Example: Base: box1, Subtract: sphere1

## Transform Components

### Move
- Category: Transform/Euclidean
- Description: Moves geometry along a vector
- Inputs:
  - G (Geometry): Geometry to move
  - T (Vector): Translation vector
- Outputs:
  - Geometry (Geometry): The moved geometry
  - Transform (Transform): The transformation
- Value Format: Geometry: geometry, Vector: {x,y,z}
- Example: Geometry: box1, Vector: {1,0,0}

### Rotate
- Category: Transform/Euclidean
- Description: Rotates geometry around an axis
- Inputs:
  - G (Geometry): Geometry to rotate
  - A (Number): Rotation angle
  - P (Plane): Rotation plane (optional)
- Outputs:
  - Geometry (Geometry): The rotated geometry
  - Transform (Transform): The transformation
- Value Format: Geometry: geometry, Angle: number, Plane: {origin, x-axis, y-axis}
- Example: Geometry: box1, Angle: 45, Plane: {0,0,0,1,0,0,0,1,0}

## Vector Components

### Vector XYZ
- Category: Vector/Vector
- Description: Creates a vector from X, Y, Z components
- Inputs:
  - X (Number): X component
  - Y (Number): Y component
  - Z (Number): Z component
- Outputs:
  - Vector (Vector): The resulting vector
  - Length (Number): The vector length
- Value Format: {x,y,z}
- Example: {1,0,0}

## Utility Components

### Number Slider
- Category: Params/Input
- Description: A slider for numeric input
- Inputs: None
- Outputs:
  - Value (Number): The slider value
- Value Format: number
- Example: 0.5

### Panel
- Category: Params/Input
- Description: Displays text or data
- Inputs: None
- Outputs:
  - Content (Text): The panel content
- Value Format: text
- Example: ""Hello World""

## Boolean Components

### Boolean Toggle
- Category: Params/Input
- Description: A toggle switch for boolean values
- Inputs: None
- Outputs:
  - Value (Boolean): The toggle state
- Value Format: true/false
- Example: true

### Stream Filter
- Category: Sets/Tree
- Description: Routes data based on a condition
- Inputs:
  - G (Integer): Gate value
  - Stream 0 (Generic): First stream
  - Stream 1 (Generic): Second stream
- Outputs:
  - Stream (Generic): Selected stream
- Value Format: Gate: integer, Streams: [stream1, stream2]
- Example: Gate: 0, Streams: [true, false]
";

        public static string GetDocumentation()
        {
            return Documentation;
        }
    }
} 