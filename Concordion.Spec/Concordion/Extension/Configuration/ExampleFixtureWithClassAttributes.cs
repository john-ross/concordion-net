﻿using System;
using System.Collections.Generic;
using System.Linq;
using Concordion.Api.Extension;

namespace Concordion.Spec.Concordion.Extension.Configuration
{
    [Extensions(typeof(FakeExtension1), typeof(FakeExtension2Factory))]
    public class ExampleFixtureWithClassAttributes
    {
    }
}
