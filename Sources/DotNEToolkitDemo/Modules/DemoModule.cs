using DotNEToolkit.Bindings;
using DotNEToolkit.Modular;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNEToolkitDemo.Modules
{
    public class DemoModule : ModuleBase
    {
        [BindableProperty]
        public List<string> StringList { get; private set; }

        [BindableProperty]
        public List<int> IntList { get; private set; }

        [BindableProperty]
        public int IntValue { get; private set; }

        [BindableProperty]
        public string StringValue { get; private set; }

        [BindableProperty]
        public List<DemoModule> Modules { get; private set; }

        protected override int OnInitialize()
        {
            return 0;
        }

        protected override void OnRelease()
        {
        }
    }
}
