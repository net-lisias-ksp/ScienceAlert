/******************************************************************************
                   Science Alert for Kerbal Space Program                    
 ******************************************************************************
    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *****************************************************************************/
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Toolbar
{
    class ApplicationLauncher : MonoBehaviour, IToolbar
    {
        // --------------------------------------------------------------------
        //    Members
        // --------------------------------------------------------------------
        public event ToolbarClickHandler OnClick;



/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/
        void Start()
        {
            
        }

        public void PlayAnimation()
        {
            
        }

        public void StopAnimation()
        {
            
        }

        public void SetUnlit()
        {
            
        }

        public void SetLit()
        {

        }


        #region properties

        public bool Important { get { return false; } set { } }

        public bool IsAnimating
        {
            get
            {
                return false;
            }
        }

        public bool IsNormal
        {
            get
            {
                return false;
            }
        }

        public bool IsLit
        {
            get
            {
                return false;
            }
        }

        public IDrawable Drawable
        {
            get
            {
                return null;
            }

            set
            {
                
            }
        }

        #endregion
    }
}
