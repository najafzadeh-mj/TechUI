using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechUI.Mvc.DropDownList
{
    public sealed class DropdownEventBuilder 
    { 
        private readonly DropdownEventOptions _options;
        internal DropdownEventBuilder(DropdownEventOptions options) 
        { 
            _options = options;
        } 
        public DropdownEventBuilder Change(string handler)
        {
            _options.Change = handler;
            return this; 
        } 
        public DropdownEventBuilder DataBound(string handler) 
        {
            _options.DataBound = handler; 
            return this;
        }
        public DropdownEventBuilder Open(string handler)
        {
            _options.Open = handler;
            return this; 
        } 
        public DropdownEventBuilder Close(string handler) 
        { 
            _options.Close = handler;
            return this; 
        } 
    }
}
