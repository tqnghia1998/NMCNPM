﻿namespace AdminClient
{
    /// <summary>
    /// A view model for <see cref="SideMenuItemControl"/>
    /// </summary>
    public class SideMenuItemViewModel: BaseViewModel
    {
        #region Public Properties

        /// <summary>
        /// Icon for menu item
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Content of side menu item
        /// </summary>
        public string Content { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public SideMenuItemViewModel()
        {

        }

        #endregion
    }
}
