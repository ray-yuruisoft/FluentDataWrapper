using FluentData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentDataWrapper
{

    public static class DbCommandExtention
    {
        public static IDictionary QueryManyAsDictionary<TEntity, TDictionary>(this FluentData.IDbCommand source)
            where TDictionary : IDictionary
        {
            if (!ReflectionHelper.IsCustomEntity<TEntity>()) throw new FluentDataException("generics type error.it must be a model of the Entity.");
            var dic = ReflectionHelper.GetProperties(typeof(TEntity))
                .Where(c => c.Value.CustomAttributes.FirstOrDefault(d => d.AttributeType.Name == "KeyofDictionary") != null
                     || c.Value.CustomAttributes.FirstOrDefault(d => d.AttributeType.Name == "ValueofDictionary") != null
                )
                .ToDictionary(c => c.Value.CustomAttributes.First(d => d.AttributeType.Name == "KeyofDictionary" || d.AttributeType.Name == "ValueofDictionary").AttributeType.Name, d => d.Value.Name);
            if (!dic.ContainsKey("KeyofDictionary")) throw new FluentDataException("model of the Entity must give a tag which indicating the key of dictionary.");
            if (!dic.ContainsKey("ValueofDictionary")) throw new FluentDataException("model of the Entity must give a tag which indicating the value of dictionary.");
            var key = dic["KeyofDictionary"];
            var value = dic["ValueofDictionary"];
            return new QueryHandler<TDictionary>(source.Data).ExecuteMany(key, value);
        }
    }

}
