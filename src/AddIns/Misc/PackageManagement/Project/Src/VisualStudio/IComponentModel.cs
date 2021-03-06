﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;

namespace Microsoft.VisualStudio.ComponentModelHost
{
	public interface IComponentModel
	{
		T GetService<T>()
			where T : class;
	}
}
