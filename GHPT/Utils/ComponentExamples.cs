using System;

namespace GHPT.Utils
{
    public static class ComponentExamples
    {
        public const string SimpleExamples = @"# Simple Grasshopper Examples

## Basic Geometry
### Creating a Box
```json
{
    ""Advice"": ""Remember to properly define the box dimensions using the number slider or the point's coordinates"",
    ""Additions"": [
        {
            ""Name"": ""Box 2Pt"",
            ""Id"": 1
        },
        {
            ""Name"": ""Point"",
            ""Id"": 2,
            ""value"": ""{0,0,0}""
        },
        {
            ""Name"": ""Point"",
            ""Id"": 3,
            ""value"": ""{10,10,10}""
        }
    ],
    ""Connections"": [
        {
            ""To"": {
                ""Id"": 1,
                ""ParameterName"": ""A""
            },
            ""From"": {
                ""Id"": 2,
                ""ParameterName"": ""Point""
            }
        },
        {
            ""To"": {
                ""Id"": 1,
                ""ParameterName"": ""B""
            },
            ""From"": {
                ""Id"": 3,
                ""ParameterName"": ""Point""
            }
        }
    ]
}
```

### Creating a Circle
```json
{
    ""Advice"": ""Use appropriate radius and plane settings for the circle"",
    ""Additions"": [
        {
            ""Name"": ""Circle CNR"",
            ""Id"": 1
        },
        {
            ""Name"": ""Plane"",
            ""Id"": 2,
            ""value"": ""{0,0,0},{1,0,0},{0,1,0}""
        },
        {
            ""Name"": ""Number Slider"",
            ""Id"": 3,
            ""value"": ""0..5..10""
        }
    ],
    ""Connections"": [
        {
            ""To"": {
                ""Id"": 1,
                ""ParameterName"": ""Plane""
            },
            ""From"": {
                ""Id"": 2,
                ""ParameterName"": ""Plane""
            }
        },
        {
            ""To"": {
                ""Id"": 1,
                ""ParameterName"": ""Radius""
            },
            ""From"": {
                ""Id"": 3,
                ""ParameterName"": ""Number""
            }
        }
    ]
}
```

### Creating a Line
```json
{
    ""Advice"": ""Define start and end points for the line"",
    ""Additions"": [
        {
            ""Name"": ""Line"",
            ""Id"": 1
        },
        {
            ""Name"": ""Point"",
            ""Id"": 2,
            ""value"": ""{0,0,0}""
        },
        {
            ""Name"": ""Point"",
            ""Id"": 3,
            ""value"": ""{10,10,10}""
        }
    ],
    ""Connections"": [
        {
            ""To"": {
                ""Id"": 1,
                ""ParameterName"": ""Start Point""
            },
            ""From"": {
                ""Id"": 2,
                ""ParameterName"": ""Point""
            }
        },
        {
            ""To"": {
                ""Id"": 1,
                ""ParameterName"": ""End Point""
            },
            ""From"": {
                ""Id"": 3,
                ""ParameterName"": ""Point""
            }
        }
    ]
}
```";

        public const string ComplexExamples = @"# Complex Grasshopper Examples

## Creating a Cup Shape
```json
{
    ""Advice"": ""Remember to properly define the cup's dimensions using the number slider"",
    ""Additions"": [
        {
            ""Name"": ""Circle CNR"",
            ""Id"": 1
        },
        {
            ""Name"": ""Move"",
            ""Id"": 2
        },
        {
            ""Name"": ""Loft"",
            ""Id"": 3
        }
    ],
    ""Connections"": [
        {
            ""To"": {
                ""Id"": 2,
                ""ParameterName"": ""Geometry""
            },
            ""From"": {
                ""Id"": 1,
                ""ParameterName"": ""Circle""
            }
        },
        {
            ""To"": {
                ""Id"": 3,
                ""ParameterName"": ""Curves""
            },
            ""From"": {
                ""Id"": 1,
                ""ParameterName"": ""Circle""
            }
        },
        {
            ""To"": {
                ""Id"": 3,
                ""ParameterName"": ""Curves""
            },
            ""From"": {
                ""Id"": 2,
                ""ParameterName"": ""Geometry""
            }
        }
    ]
}
```

## Creating a Twisty Skyscraper
```json
{
    ""Advice"": ""Make sure to use reasonable inputs or the skyscraper will look weird"",
    ""Additions"": [
        {
            ""Name"": ""Rectangle"",
            ""Id"": 1
        },
        {
            ""Name"": ""Number Slider"",
            ""Id"": 2,
            ""Value"": ""0..50..100""
        },
        {
            ""Name"": ""Unit Z"",
            ""Id"": 3
        },
        {
            ""Name"": ""Extrusion"",
            ""Id"": 4
        },
        {
            ""Name"": ""Number Slider"",
            ""Id"": 5,
            ""Value"": ""0..90..360""
        },
        {
            ""Name"": ""Line"",
            ""Id"": 6
        },
        {
            ""Name"": ""Point"",
            ""Id"": 7,
            ""value"": ""{0,0,0}""
        },
        {
            ""Name"": ""Point"",
            ""Id"": 8,
            ""value"": ""{0,0,250}""
        },
        {
            ""Name"": ""Twist"",
            ""Id"": 9
        },
        {
            ""Name"": ""Solid Union"",
            ""Id"": 10
        },
        {
            ""Name"": ""Brep Join"",
            ""Id"": 11
        }
    ],
    ""Connections"": [
        {
            ""To"": {
                ""Id"": 4,
                ""ParameterName"": ""Base""
            },
            ""From"": {
                ""Id"": 1,
                ""ParameterName"": ""Rectangle""
            }
        },
        {
            ""To"": {
                ""Id"": 3,
                ""ParameterName"": ""Factor""
            },
            ""From"": {
                ""Id"": 2,
                ""ParameterName"": ""Number""
            }
        },
        {
            ""To"": {
                ""Id"": 9,
                ""ParameterName"": ""Angle""
            },
            ""From"": {
                ""Id"": 5,
                ""ParameterName"": ""Number""
            }
        },
        {
            ""To"": {
                ""Id"": 4,
                ""ParameterName"": ""Direction""
            },
            ""From"": {
                ""Id"": 3,
                ""ParameterName"": ""Unit vector""
            }
        },
        {
            ""To"": {
                ""Id"": 9,
                ""ParameterName"": ""Geometry""
            },
            ""From"": {
                ""Id"": 4,
                ""ParameterName"": ""Extrusion""
            }
        },
        {
            ""To"": {
                ""Id"": 6,
                ""ParameterName"": ""Start Point""
            },
            ""From"": {
                ""Id"": 7,
                ""ParameterName"": ""Point""
            }
        },
        {
            ""To"": {
                ""Id"": 6,
                ""ParameterName"": ""End Point""
            },
            ""From"": {
                ""Id"": 8,
                ""ParameterName"": ""Point""
            }
        },
        {
            ""To"": {
                ""Id"": 10,
                ""ParameterName"": ""Breps""
            },
            ""From"": {
                ""Id"": 9,
                ""ParameterName"": ""Geometry""
            }
        },
        {
            ""To"": {
                ""Id"": 11,
                ""ParameterName"": ""Breps""
            },
            ""From"": {
                ""Id"": 10,
                ""ParameterName"": ""Result""
            }
        },
        {
            ""To"": {
                ""Id"": 9,
                ""ParameterName"": ""Axis""
            },
            ""From"": {
                ""Id"": 6,
                ""ParameterName"": ""Line""
            }
        }
    ]
}
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