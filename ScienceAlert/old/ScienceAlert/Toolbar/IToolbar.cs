///******************************************************************************
//                   Science Alert for Kerbal Space Program                    
// ******************************************************************************
//    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// *****************************************************************************/
//using UnityEngine;

//namespace ScienceAlert.Toolbar
//{
//    public delegate void ToolbarClickHandler(ClickInfo click);

//    /// <summary>
//    /// Common interface shared by blizzy's toolbar interface and the
//    /// application launcher interface
//    /// </summary>
//    public interface IToolbar
//    {
//        void PlayAnimation();
//        void StopAnimation();

//        void SetUnlit();
//        void SetLit();

//        IDrawable Drawable
//        {
//            get;
//            set;
//        }

//        bool Important
//        {
//            get;
//            set;
//        }

//        bool IsAnimating { get; }                   // animation is playing
//        bool IsNormal { get; }                      // no animation, not lit up -- normal flask texture
//        bool IsLit { get; }                         // animation not playing but flask icon active

//        event ToolbarClickHandler OnClick;
//    }



//    /// <summary>
//    /// This gets passed around to listeners during a click event
//    /// </summary>
//    public class ClickInfo
//    {
//        public int button;
//        public bool used;

//        public ClickInfo() { button = 0; used = false; }
//        public void Consume() { used = true; }
//        public bool Unused { get { return !used; } }
//    }
//}
