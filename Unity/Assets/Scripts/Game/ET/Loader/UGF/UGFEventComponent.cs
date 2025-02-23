using System;
using System.Collections.Generic;

namespace ET
{
    [Code]
    [EnableMethod]
    [ComponentOf]
    public sealed class UGFEventComponent : Singleton<UGFEventComponent>, ISingletonAwake
    {
        private readonly Dictionary<int, IUGFUIFormEvent> m_UIFormEvents = new ();
        private readonly Dictionary<string, IUGFEntityEvent> m_EntityEvents = new ();

        public bool TryGetUIFormEvent(int uiFormId, out IUGFUIFormEvent uiFormEvent)
        {
            return this.m_UIFormEvents.TryGetValue(uiFormId, out uiFormEvent);
        }
        
        public bool TryGetEntityEvent(string entityEventTypeName, out IUGFEntityEvent entityEvent)
        {
            return this.m_EntityEvents.TryGetValue(entityEventTypeName, out entityEvent);
        }

        public IUGFUIFormEvent GetUIFormEvent(int uiFormId)
        {
            return m_UIFormEvents[uiFormId];
        }

        public IUGFEntityEvent GetEntityEvent(string entityEventTypeName)
        {
            return m_EntityEvents[entityEventTypeName];
        }

        public void Awake()
        {
            this.m_UIFormEvents.Clear();
            HashSet<Type> uiEventAttributes = CodeTypes.Instance.GetTypes(typeof(UGFUIFormEventAttribute));
            foreach (Type type in uiEventAttributes)
            {
                object[] attrs = type.GetCustomAttributes(typeof(UGFUIFormEventAttribute), false);
                UGFUIFormEventAttribute ugfUIFormEventAttribute = (UGFUIFormEventAttribute)attrs[0];
                IUGFUIFormEvent ugfUIFormEvent = Activator.CreateInstance(type) as IUGFUIFormEvent;
                foreach (int uiFormId in ugfUIFormEventAttribute.uiFormIds)
                {
                    this.m_UIFormEvents.Add(uiFormId, ugfUIFormEvent);
                }
            }
            
            this.m_EntityEvents.Clear();
            var types = CodeTypes.Instance.GetTypes();
            Type entityEventType = typeof(IUGFEntityEvent);
            foreach (Type type in types.Values)
            {
                if (type.IsGenericType || type.IsAbstract || !entityEventType.IsAssignableFrom(type))
                {
                    continue;
                }
                IUGFEntityEvent ugfEntityEvent = Activator.CreateInstance(type) as IUGFEntityEvent;
                this.m_EntityEvents.Add(type.FullName, ugfEntityEvent);
            }
        }
    }
}
