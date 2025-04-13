using System;

namespace GHPT.Utils
{
    public static class ComponentExamples
    {
        public const string SimpleExamples = @"# Simple Grasshopper Examples

## Basic Geometry
### Creating Points
```python
# Create a point at (0,0,0)
point = Point3d(0,0,0)

# Create multiple points
points = [Point3d(x,0,0) for x in range(10)]
```

### Creating Lines
```python
# Create a line between two points
start = Point3d(0,0,0)
end = Point3d(10,0,0)
line = Line(start, end)

# Create multiple lines
lines = [Line(Point3d(x,0,0), Point3d(x,10,0)) for x in range(10)]
```

## Basic Transformations
### Moving Geometry
```python
# Move a point
point = Point3d(0,0,0)
vector = Vector3d(10,0,0)
moved_point = point + vector

# Move multiple points
points = [Point3d(x,0,0) for x in range(10)]
moved_points = [p + vector for p in points]
```

### Rotating Geometry
```python
# Rotate a point around Z axis
point = Point3d(10,0,0)
angle = 45 # degrees
rotated_point = point.Rotate(angle, Vector3d.ZAxis, Point3d.Origin)
```

## Basic Data Operations
### Creating Series
```python
# Create a series of numbers
numbers = [x for x in range(10)]

# Create a series with step
numbers = [x * 0.5 for x in range(10)]
```

### Basic Math Operations
```python
# Add two numbers
a = 5
b = 3
result = a + b

# Multiply two numbers
result = a * b
```";

        public const string ComplexExamples = @"# Complex Grasshopper Examples

## Advanced Geometry
### Creating Surfaces
```python
# Create a surface from points
points = [Point3d(x,y,0) for x in range(10) for y in range(10)]
surface = NurbsSurface.CreateFromPoints(points, 10, 10, 3, 3)

# Create a lofted surface
curves = [Line(Point3d(x,0,0), Point3d(x,10,0)) for x in range(10)]
surface = Brep.CreateFromLoft(curves, Point3d.Origin, Point3d.Origin, LoftType.Normal, False)
```

### Creating Complex Curves
```python
# Create a NURBS curve
points = [Point3d(x, math.sin(x), 0) for x in range(10)]
curve = NurbsCurve.CreateFromPoints(points, 3)

# Create a polyline with specific points
points = [Point3d(x, x*x, 0) for x in range(10)]
polyline = Polyline(points)
```

## Advanced Transformations
### Multiple Transformations
```python
# Move and rotate a point
point = Point3d(10,0,0)
vector = Vector3d(0,10,0)
angle = 45

# First move
moved_point = point + vector
# Then rotate
rotated_point = moved_point.Rotate(angle, Vector3d.ZAxis, Point3d.Origin)
```

### Scaling with Center
```python
# Scale geometry around a center point
point = Point3d(10,0,0)
center = Point3d(5,0,0)
scale = 2.0

# Scale relative to center
scaled_point = center + (point - center) * scale
```

## Advanced Data Operations
### Data Trees
```python
# Create a data tree
tree = DataTree<Point3d>()
for i in range(10):
    path = GH_Path(i)
    points = [Point3d(x,i,0) for x in range(10)]
    tree.AddRange(points, path)

# Access data tree
for i in range(tree.BranchCount):
    points = tree.Branch(i)
    # Process points
```

### Conditional Operations
```python
# Filter points based on condition
points = [Point3d(x,0,0) for x in range(10)]
filtered_points = [p for p in points if p.X > 5]

# Transform points based on condition
transformed_points = [p + Vector3d(0,10,0) if p.X > 5 else p for p in points]
```";

        public static string GetSimpleExamples()
        {
            return SimpleExamples;
        }

        public static string GetComplexExamples()
        {
            return ComplexExamples;
        }
    }
} 