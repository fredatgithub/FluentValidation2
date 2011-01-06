#region License
// Copyright (c) Jeremy Skinner (http://www.jeremyskinner.co.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fluentvalidation.codeplex.com
#endregion

namespace FluentValidation.Mvc {
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Web.Mvc;

	public class CustomizeValidatorAttribute : CustomModelBinderAttribute, IModelBinder {
		public string RuleSet { get; set; }
		public string Properties { get; set; }

		public string[] GetProperties() {
			if(string.IsNullOrEmpty(Properties)) {
				return new string[0];
			}

			return Properties.Split(',', ';');
		}

		public override IModelBinder GetBinder() {
			return this;
		}

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
			// Originally I thought about storing this inside ModelMetadata.AdditionalValues.
			// Unfortunately, DefaultModelBinder overwrites this property internally.
			// So anything added to AdditionalValues will not be passed to the ValidatorProvider.
			// This is a very poor design decision. 
			// The only piece of information that is passed all the way down to the validator is the controller context.
			// So we resort to custom ControllerContext to store this metadata. 
			// Horrible, horrible, horrible hack. Horrible.
			var wrappedContext = new ControllerContextHackery(controllerContext, this);

			var innerBinder = ModelBinders.Binders.GetBinder(bindingContext.ModelType);
			return innerBinder.BindModel(wrappedContext, bindingContext);
		}

		public static CustomizeValidatorAttribute GetFromControllerContext(ControllerContext context) {
			var hack = context as ControllerContextHackery;
			return hack != null ? hack.Attribute : null;
		}

		private class ControllerContextHackery : ControllerContext {
			public CustomizeValidatorAttribute Attribute { get; private set; }

			public ControllerContextHackery(ControllerContext controllerContext, CustomizeValidatorAttribute attribute) : base(controllerContext) {
				Attribute = attribute;
			}
		}

	}
}