﻿using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AdminClient
{
    /// <summary>
    /// Interaction logic for PageHost.xaml
    /// </summary>
    public partial class PageHost : UserControl
    {
        #region Dependency Properties

        /// <summary>
        /// The current page to show in the page host
        /// </summary>
        public ApplicationPage CurrentPage
        {
            get => (ApplicationPage)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        /// <summary>
        /// Register <see cref="CurrentPage"/> as a dependency property
        /// </summary>
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(ApplicationPage), typeof(PageHost), new UIPropertyMetadata(default(ApplicationPage), null, new CoerceValueCallback(CurrentPagePropertyChanged)));

        /// <summary>
        /// The current page to show in the page host
        /// </summary>
        public BaseViewModel CurrentPageViewModel
        {
            get => (BaseViewModel)GetValue(CurrentPageViewModelProperty);
            set => SetValue(CurrentPageViewModelProperty, value);
        }

        /// <summary>
        /// Register <see cref="CurrentPageViewModel"/> as a dependency property
        /// </summary>
        public static readonly DependencyProperty CurrentPageViewModelProperty =
            DependencyProperty.Register("CurrentPageViewModel", typeof(BaseViewModel), typeof(PageHost), new UIPropertyMetadata());

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public PageHost()
        {
            InitializeComponent();

            // If we are in design mode, show the current page
            // as the dependency property does not fire
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                NewPage.Content = IoC.Application.CurrentPage.ToBasePage();
            }
        }

        #endregion

        #region Property Changed Events

        /// <summary>
        /// Called when the <see cref="CurrentPage"/> value has changed
        /// </summary>
        /// <param name="d"></param>
        /// <param name="value"></param>
        private static object CurrentPagePropertyChanged(DependencyObject d, object value)
        {
            // Get current value
            var currentPage = (ApplicationPage)d.GetValue(CurrentPageProperty);
            var currentPageViewModel = d.GetValue(CurrentPageViewModelProperty);

            // Get the frames
            var newPageFrame = (d as PageHost).NewPage;
            var oldPageFrame = (d as PageHost).OldPage;
            BasePage temp = newPageFrame.Content as BasePage;

            // If the current page wasn't changed
            // just update the view model
            if ((newPageFrame.Content is BasePage page) && page.ToApplicationPage() == (ApplicationPage)value)
            {
                if (currentPageViewModel != null)
                {
                    // Just update the view model
                    page.ViewModelObject = currentPageViewModel;
                }

                return value;
            }

            // Store the current page content as the old page
            var oldPageContent = newPageFrame.Content;

            // Remove current page from new page frame
            newPageFrame.Content = null;

            // Move the previous page into old page frame
            oldPageFrame.Content = oldPageContent;

            // Animate out previous page when the Loaded event fires
            // right after this call due to moving frame
            if (oldPageContent is BasePage oldPage)
            {
                // Once it is done, remove it
                Task.Delay((int)(0)).ContinueWith((t) =>
                {
                    // Remove old page
                    Application.Current.Dispatcher.Invoke(() => oldPageFrame.Content = null);
                });
            }

            //Set the new page content
            newPageFrame.Content = currentPage.ToBasePage(currentPageViewModel);

            return value;

        }

        #endregion
    }
}
