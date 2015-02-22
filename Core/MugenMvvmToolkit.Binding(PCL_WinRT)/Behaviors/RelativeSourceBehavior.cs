#region Copyright

// ****************************************************************************
// <copyright file="RelativeSourceBehavior.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Binding.Sources;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    public sealed class RelativeSourceBehavior : BindingBehaviorBase
    {
        #region Nested types

        private sealed class ParentSourceValue : ISourceValue, IEventListener
        {
            #region Fields

            private readonly bool _isElementSource;
            private readonly IRelativeSourceExpressionNode _node;
            private readonly IDisposable _subscriber;
            private readonly WeakReference _targetReference;
            private bool _hasParent;
            private WeakReference _value;

            #endregion

            #region Constructors

            public ParentSourceValue(object target, IRelativeSourceExpressionNode node)
            {
                _node = node;
                _isElementSource = _node.Type == RelativeSourceExpressionNode.ElementSourceType;
                _targetReference = ServiceProvider.WeakReferenceFactory(target, true);
                _value = Empty.WeakReference;
                IBindingMemberInfo rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(target.GetType());
                if (rootMember != null)
                    _subscriber = rootMember.TryObserve(target, this);
                TryHandle(null, null);
            }

            #endregion

            #region Implementation of interfaces

            public bool IsWeak
            {
                get { return true; }
            }

            public bool TryHandle(object sender, object message)
            {
                object target = _targetReference.Target;
                if (target == null)
                {
                    Value = null;
                    if (_subscriber != null)
                        _subscriber.Dispose();
                    return false;
                }

                IVisualTreeManager treeManager = BindingServiceProvider.VisualTreeManager;
                _hasParent = treeManager.FindParent(target) != null;
                Value = _isElementSource
                    ? treeManager.FindByName(target, _node.ElementName)
                    : treeManager.FindRelativeSource(target, _node.Type, _node.Level);
                return true;
            }

            public object Value
            {
                get
                {
                    object target = _value.Target;
                    if (_hasParent && target == null)
                    {
                        if (_isElementSource)
                            Tracer.Warn(BindingExceptionManager.ElementSourceNotFoundFormat2, _targetReference.Target,
                                _node.ElementName);
                        else
                            Tracer.Warn(BindingExceptionManager.RelativeSourceNotFoundFormat3, _targetReference.Target,
                                _node.Type, _node.Level);
                    }
                    return target ?? BindingConstants.UnsetValue;
                }
                private set
                {
                    if (Equals(value, _value.Target))
                        return;
                    _value = ToolkitExtensions.GetWeakReferenceOrDefault(value, Empty.WeakReference, false);
                    EventHandler<ISourceValue, EventArgs> handler = ValueChanged;
                    if (handler != null)
                        handler(this, EventArgs.Empty);
                }
            }

            public bool IsAlive
            {
                get { return _targetReference.Target != null; }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion
        }

        #endregion

        #region Fields

        private readonly Guid _id;
        private readonly IRelativeSourceExpressionNode _relativeSourceNode;
        private IBindingSource _bindingSource;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelativeSourceBehavior" /> class.
        /// </summary>
        public RelativeSourceBehavior(IRelativeSourceExpressionNode relativeSource)
        {
            _id = Guid.NewGuid();
            _relativeSourceNode = relativeSource;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the additional path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Gets the relative source node.
        /// </summary>
        public IRelativeSourceExpressionNode RelativeSourceNode
        {
            get { return _relativeSourceNode; }
        }

        /// <summary>
        ///     Gets the current <see cref="IBindingSource" />.
        /// </summary>
        public IBindingSource BindingSource
        {
            get { return _bindingSource; }
        }

        /// <summary>
        ///     Gets the current value.
        /// </summary>
        [CanBeNull]
        public object Value
        {
            get
            {
                if (BindingSource == null)
                    return null;
                IBindingPathMembers pathMembers = BindingSource.GetPathMembers(true);
                object value = pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null);
                if (value.IsUnsetValue())
                    return null;
                var memberValue = value as BindingMemberValue;
                if (memberValue == null)
                    return value;
                return memberValue.GetValue(null);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Updates the relative source value.
        /// </summary>
        public static IBindingSource GetBindingSource(IRelativeSourceExpressionNode node, [NotNull] object target,
            string pathEx)
        {
            if (target == null)
                throw BindingExceptionManager.InvalidBindingTarget(node.Path);
            string path = node.Path ?? String.Empty;
            if (!String.IsNullOrEmpty(pathEx))
                path = BindingExtensions.MergePath(path, pathEx);

            if (node.Type != RelativeSourceExpressionNode.SelfType)
            {
                if (node.Type == RelativeSourceExpressionNode.ContextSourceType)
                    target = BindingServiceProvider.ContextManager.GetBindingContext(target);
                else
                    target = new ParentSourceValue(target, node);
            }
            IObserver observer = BindingServiceProvider.ObserverProvider.Observe(target,
                BindingServiceProvider.BindingPathFactory(path), false);
            return new BindingSource(observer);
        }

        #endregion

        #region Overrides of BindingBehaviorBase

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public override Guid Id
        {
            get { return _id; }
        }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public override int Priority
        {
            get { return int.MaxValue; }
        }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            OnDetached();
            _bindingSource = GetBindingSource(RelativeSourceNode, Binding.TargetAccessor.Source.GetSource(true), Path);
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected override void OnDetached()
        {
            if (BindingSource != null)
                BindingSource.Dispose();
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        protected override IBindingBehavior CloneInternal()
        {
            return new RelativeSourceBehavior(RelativeSourceNode);
        }

        #endregion
    }
}