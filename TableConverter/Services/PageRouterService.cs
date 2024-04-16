using Avalonia.Controls;
using System;
using System.Collections.Generic;
using TableConverter.ViewModels;

namespace TableConverter.Services
{
    public class PageRouterService
    {
        protected Action<Control>? UpdateViewFunc { get; set; } = null;
        protected Stack<ViewModelBase> ViewHistory = new();

        public void Initialise(Action<Control> update_view_function)
        {
            UpdateViewFunc = update_view_function;
        }

        public void NavigatePage<TViewModel>()
        {
            if (Activator.CreateInstance(typeof(TViewModel)) is ViewModelBase view_model)
            {
                ViewHistory.Push(view_model);

                if (InitialiseView(view_model) is Control view)
                {
                    UpdateViewFunc?.Invoke(view);
                }
            }
        }

        public void NavigateBack()
        {
            if (ViewHistory.Count > 1)
            { 
                ViewHistory.Pop();

                if (ViewHistory.TryPeek(out var view_model))
                {
                    if (InitialiseView(view_model) is Control view)
                    {
                        UpdateViewFunc?.Invoke(view);
                    }
                }
            }
        }

        protected virtual Control? InitialiseView(object? view_model)
        {
            if (view_model is ViewModelBase view_model_base)
            {
                var type = Type.GetType(view_model_base.GetType().FullName!.Replace("ViewModel", "View"));

                if (type is not null)
                {
                    var view = (Control)Activator.CreateInstance(type)!;

                    view.DataContext = view_model_base;

                    return view;
                }
            }

            return null;
        }
    }
}
