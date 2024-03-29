using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;

namespace OdataToEntity.EfCore.DynamicDataContext
{
    public sealed class DynamicTypeDefinition
    {
        private readonly Dictionary<INavigation, Type> _navigations;
        private readonly SortedDictionary<string, Type> _snavigations;

        public DynamicTypeDefinition(Type dynamicTypeType, in TableFullName tableFullName, bool isQueryType, String tableEdmName)
        {
            DynamicTypeType = dynamicTypeType;
            TableFullName = tableFullName;
            TableEdmName = tableEdmName;
            IsQueryType = isQueryType;

            _navigations = new Dictionary<INavigation, Type>();
            _snavigations = new SortedDictionary<string, Type>();
        }

        internal void AddNavigationProperty(INavigation navigation, Type clrType)
        {
            if (!_navigations.TryGetValue(navigation, out _))
            {
                _navigations.Add(navigation, clrType);
                _snavigations.Add($"{navigation.Name}_{navigation.ForeignKey.PrincipalEntityType.Name}_{navigation.ForeignKey.PrincipalKey}_{navigation.ForeignKey.DeclaringEntityType.Name}_{navigation.ForeignKey.Properties.First().Name}", clrType);
            }
                
        }
        public Type GetNavigationPropertyClrType(INavigation navigation)
        {
            return _snavigations[$"{navigation.Name}_{navigation.ForeignKey.PrincipalEntityType.Name}_{navigation.ForeignKey.PrincipalKey}_{navigation.ForeignKey.DeclaringEntityType.Name}_{navigation.ForeignKey.Properties.First().Name}"];
            return _navigations.FirstOrDefault(x => x.Key.ForeignKey.ToString() == navigation.ForeignKey.ToString()).Value;
            return _navigations[navigation];
        }

        public Type DynamicTypeType { get; }
        public bool IsQueryType { get; }
        public String TableEdmName { get; }
        public TableFullName TableFullName { get; }
    }
}
