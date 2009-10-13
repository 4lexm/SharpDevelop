﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Markup;

namespace SharpDevelop.XamlDesigner.Dom
{
	public class PropertyId : MemberId
	{
		internal PropertyId(PropertyDescriptor d)
		{
			Descriptor = d;
		}

		public PropertyDescriptor Descriptor { get; private set; }

		public override string Name
		{
			get { return Descriptor.Name; }
		}

		public override Type OwnerType
		{
			get { return Descriptor.ComponentType; }
		}

		public override Type ValueType
		{
			get { return Descriptor.PropertyType; }
		}

		public override bool IsReadOnly
		{
			get { return Descriptor.IsReadOnly; }
		}

		public override ValueSerializer ValueSerializer
		{
			get { return ValueSerializer.GetSerializerFor(Descriptor); }
		}
	}
}