﻿// Copyright (c) Microsoft Corporation
// All rights reserved

namespace Microsoft.VisualStudio.Text.Tagging
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.VisualStudio.Text.Utilities;
    using Microsoft.VisualStudio.Utilities;

    /// <summary>
    /// The metadata interface for exporters and importers of metadata on <see cref="ITaggerProvider"/> factories.
    /// </summary>
    public interface INamedTaggerMetadata : ITaggerMetadata, INamedContentTypeMetadata
    {
    }
}
