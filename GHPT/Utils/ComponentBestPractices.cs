using System;

namespace GHPT.Utils
{
    public static class ComponentBestPractices
    {
        public const string BestPractices = @"# Grasshopper Component Best Practices

## Core Principles
1. **Foundation with Points and Planes**
   - Begin all geometry definitions using Point and Plane components
   - Points provide precise locations in 3D space
   - Planes offer orientation and serve as local coordinate systems

2. **Simplicity and Specificity**
   - Use the most appropriate components for each task
   - Avoid unnecessary complexity in component chains

3. **Data Type Consistency**
   - Ensure inputs and outputs maintain consistent data types
   - Prevent errors by matching data types across connections

4. **Input Awareness**
   - If a component has required inputs, ensure that they are given.
   - Often times circle or retangle components will require planes or points as input.
   - You muct construct these inputs or the component will throw an error.

5. **Logical Organization**
   - Group related components together
   - Maintain a clear, readable layout on the canvas

6. **Use of Defaults and Validation**
   - Leverage default parameter values where appropriate
   - Validate inputs before connecting components

## Canvas Layout and Organization
1. **Component Placement**
   - Position input components on the left
   - Place processing components in the center
   - Position output components on the right
   - Align related components vertically

2. **Spacing and Boundaries**
   - Maintain consistent spacing (100 units) between components
   - Keep components within visible canvas area
   - Use positive X and Y coordinates (0-1000 range)

3. **Connection Routing**
   - Draw wires as straight as possible
   - Avoid overlaps and crossings
   - Group related connections together
   - Minimize wire lengths

## Naming Conventions and Parameter Formatting
1. **Component Naming**
   - Use exact component names as they appear in the doumentation

2. **Parameter Formatting**
   - Points: {x,y,z}
   - Number Slider ranges: min..value..max
   - Series values: start..step..end
   - Boolean values: true or false
   - Text values: ""example""

## Connection Validation
1. **Type Compatibility**
   - Ensure matching data types across connections
   - Example: Number → Number, Point → Point

2. **Parameter Naming**
   - Use descriptive names for inputs (e.g., ""input"", ""geometry"")
   - Use clear names for outputs (e.g., ""output"", ""result"")

3. **Required Parameters**
   - Connect all mandatory inputs
   - Validate connections before execution

## Common Workflows
1. **Geometry Creation**
   - Basic Shapes:
     - Use Point components for specific locations
     - Construct lines, circles, rectangles, and boxes
   - Complex Forms:
     - Use Interpolate Curve, Loft, Surface from Points
     - Reference points and planes for positioning

2. **Transformations**
   - Basic Operations:
     - Move, Rotate, and Scale components
     - Use planes to define axes and centers
   - Advanced Techniques:
     - Orient, Morph, and Twist components
     - Use planes for orientation guidance

3. **Boolean Operations**
   - Solid Manipulations:
     - Solid Union, Difference, and Intersection
     - Ensure clean, properly aligned geometries
   - Surface Operations:
     - Surface Split, Trim, and Join
     - Maintain planarity and continuity

## Error Handling and Validation
1. **Component Checks**
   - Verify component existence and configuration
   - Ensure all required inputs are connected
   - Validate input types

2. **Connection Integrity**
   - Avoid circular dependencies
   - Confirm logical data flow
   - Validate parameter ranges and formats

## Common Pitfalls
1. **Missing Inputs**
   - Connect all required parameters
   - Avoid component errors and unexpected results

2. **Type Mismatches**
   - Prevent incompatible data type connections
   - Example: Don't connect numbers to point inputs

3. **Invalid Geometry**
   - Ensure watertight geometry for boolean operations
   - Verify proper geometry definition
   - Check for incomplete outcomes";

        public static string GetBestPractices()
        {
            return BestPractices;
        }
    }
}