﻿#region Copyright

// ****************************************************************************
// <copyright file="IViewModelAwareView.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Views
{
    [Preserve(AllMembers = true)]
    public interface IViewModelAwareView<TViewModel> : IView where TViewModel : IViewModel
    {
        TViewModel ViewModel { get; set; }
    }
}
