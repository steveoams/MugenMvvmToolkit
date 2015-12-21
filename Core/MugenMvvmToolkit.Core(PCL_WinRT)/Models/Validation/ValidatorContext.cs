﻿#region Copyright

// ****************************************************************************
// <copyright file="ValidatorContext.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Validation;

namespace MugenMvvmToolkit.Models.Validation
{
    public class ValidatorContext : IValidatorContext
    {
        #region Fields

        private readonly ICollection<string> _ignoreProperties;
        private readonly object _instance;
        private readonly IDictionary<string, ICollection<string>> _propertyMappings;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataContext _validationMetadata;

        #endregion

        #region Constructors

        public ValidatorContext(object instanceToValidate, IServiceProvider serviceProvider = null)
            : this(instanceToValidate, null, null, null, serviceProvider)
        {
        }

        public ValidatorContext(object instanceToValidate, IDictionary<string, ICollection<string>> propertyMappings,
            ICollection<string> ignoredProperties, IDataContext validationMetadata = null,
            IServiceProvider serviceProvider = null)
        {
            Should.NotBeNull(instanceToValidate, nameof(instanceToValidate));
            _instance = instanceToValidate;
            _propertyMappings = propertyMappings ?? new Dictionary<string, ICollection<string>>();
            _ignoreProperties = ignoredProperties ?? new HashSet<string>();
            _validationMetadata = validationMetadata ?? new DataContext();
            _serviceProvider = serviceProvider;
        }

        #endregion

        #region Implementation of IValidatorContext

        public object Instance
        {
            get { return _instance; }
        }

        public IDataContext ValidationMetadata
        {
            get { return _validationMetadata; }
        }

        public IDictionary<string, ICollection<string>> PropertyMappings
        {
            get { return _propertyMappings; }
        }

        public ICollection<string> IgnoreProperties
        {
            get { return _ignoreProperties; }
        }

        public IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }

        #endregion

        #region Implementation of IServiceProvider

        object IServiceProvider.GetService(Type serviceType)
        {
            if (ServiceProvider == null)
                return null;
            return ServiceProvider.GetService(serviceType);
        }

        #endregion
    }
}
