//using System;
//using strange.extensions.command.impl;
//using strange.extensions.injector.api;

//namespace ScienceAlert.VesselContext.Experiments.Rules.Old
//{
//// ReSharper disable once ClassNeverInstantiated.Global
//    public class CommandCreateRuleTypeBindings : Command
//    {
//        private readonly IInjectionBinder _binder;
//        private readonly IExperimentRuleTypeProvider _typeProvider;

//        public CommandCreateRuleTypeBindings(IInjectionBinder binder, IExperimentRuleTypeProvider typeProvider)
//        {
//            if (binder == null) throw new ArgumentNullException("binder");
//            if (typeProvider == null) throw new ArgumentNullException("typeProvider");
//            _binder = binder;
//            _typeProvider = typeProvider;
//        }


//        public override void Execute()
//        {
//            foreach (var ruleType in _typeProvider.Get())
//                _binder.Bind(ruleType).To(ruleType);
//        }
//    }
//}
