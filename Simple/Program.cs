using FluentDataWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Simple
{
    class Program
    {
        static void Main(string[] args)
        {
            //var a = sum_shop_productPrice.sumOneMonth(new DateTime(2018, 1, 12));
            //var b = sum_shop_productPrice.sumOneDay(new DateTime(2018, 1, 30));

            shop_productOrderData<shop_productOrderModle> shop_ProductOrderData = new shop_productOrderData<shop_productOrderModle>();
            var c = shop_ProductOrderData.Context().Select<shop_productOrderModle>("sysNo,agentSysNo,createTime").From("shop_productOrder").QueryMany();
            shop_ProductOrderData.Context().Dispose();
            var e = shop_ProductOrderData.SelectList("select sysno,agentSysNo,createTime from shop_productOrder");
            var d = shop_ProductOrderData.Context().Sql("select sysno,agentSysNo,createTime from shop_productOrder").QueryMany<shop_productOrderModle>();

            Console.ReadKey();
        }
    }

    public class sum_shop_productPrice
    {

        private static bool sumProcess(DateTime dateTime)
        {
            var time = dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

            #region  query three tables to memery

            shop_agentData<shop_agentModle> shop_AgentData = new shop_agentData<shop_agentModle>();
            var shop_AgentDatas = shop_AgentData.SelectList("select sysNo,businessmobile from shop_agent");
            var shop_AgentDatasFilter = shop_AgentDatas
                                            .Select(c => c.sysNo)
                                            .ToList()
                                            ;

            shop_productOrderData<shop_productOrderModle> shop_ProductOrderData = new shop_productOrderData<shop_productOrderModle>();
            var shop_ProductOrders = shop_ProductOrderData
                .SelectList(string.Format("SELECT sysNo,agentSysNo,createTime from shop_productOrder WHERE DATEDIFF(createTime,'{0}') = 0 and isPay = 1", time))
                .ToList()
                ;
            var shop_ProductOrdersFileter = shop_ProductOrders
                                                    .Select(c => c.sysNo)
                                                    .ToList()
                                                    ;

            shop_productData<shop_productModle> shop_ProductData = new shop_productData<shop_productModle>();
            var shop_ProductDatas = shop_ProductData.SelectList("select sysNo,gameId from shop_product");

            #endregion

            #region get shop_productOrderChildModle which has been dealt

            shop_productOrderChildData<shop_productOrderChildModle> shop_ProductOrderChildData = new shop_productOrderChildData<shop_productOrderChildModle>();
            var shop_ProductOrderChildDatas = shop_ProductOrderChildData
                 .SelectList(string.Format("SELECT sysNo,orderSysNo,productSysNo,productMemberPrice,productCount from shop_productOrderChild WHERE DATEDIFF(createTime,'{0}') = 0", time))
                 //根据shop_ProductOrders表筛除不符合条件的数据
                 .Where(c => shop_ProductOrdersFileter.Contains(c.orderSysNo))
                //关联 shop_ProductDatas 得到 gameId ;关联 shop_ProductOrders 得到 agentSysNo   
                .Select(c => new
                {
                    c.sysNo,
                    c.orderSysNo,
                    c.productSysNo,
                    gameId = (from b in shop_ProductDatas
                              where b.sysNo == c.productSysNo
                              select b.gameId)
                                .FirstOrDefault(),
                    agentSysNo = (from b in shop_ProductOrders
                                  where b.sysNo == c.orderSysNo
                                  select b.agentSysNo)
                                   .FirstOrDefault(),
                    c.productMemberPrice,
                    c.productCount,
                    createTime = (from b in shop_ProductOrders
                                  where b.sysNo == c.orderSysNo
                                  select b.createTime)
                                   .FirstOrDefault()
                })
                //关联 shop_AgentDatas 得到 businessmobile
                .Select(c => new
                {
                    c.sysNo,
                    c.orderSysNo,
                    c.productSysNo,
                    c.gameId,
                    c.agentSysNo,
                    c.productMemberPrice,
                    c.productCount,
                    c.createTime,
                    businessmobile = (from b in shop_AgentDatas
                                      where b.sysNo == c.agentSysNo
                                      select b.businessmobile)
                                   .FirstOrDefault()
                })
                //过滤掉businessmobile为空的情况
                .Where(c => c.businessmobile != null)
                .ToList()
                ;

            #endregion

            #region calculate

            List<sum_shop_productPriceModel> sum_shop_productPriceModels = new List<sum_shop_productPriceModel>();
            foreach (var item in shop_ProductOrderChildDatas)
            {
                var sum_Shop_productPriceModel = sum_shop_productPriceModels.Find(c => c.businessmobile == item.businessmobile && c.gameId == item.gameId);
                if (sum_Shop_productPriceModel == null)
                {
                    sum_Shop_productPriceModel = new sum_shop_productPriceModel()
                    {
                        businessmobile = item.businessmobile,
                        gameId = item.gameId,
                        priceTotal = item.productCount * item.productMemberPrice,
                        orderCreateTime = item.createTime
                    };
                    sum_shop_productPriceModels.Add(sum_Shop_productPriceModel);
                }
                else
                {
                    sum_Shop_productPriceModel.priceTotal += item.productCount * item.productMemberPrice;
                }
            }

            #endregion

            #region into database

            sum_shop_productPriceData<sum_shop_productPriceModel> sum_Shop_ProductPriceData = new sum_shop_productPriceData<sum_shop_productPriceModel>();
            var t = sum_Shop_ProductPriceData.SelectList();
            var res = true;
            if (sum_shop_productPriceModels.Count == 0) res = false;//如果没有数据更新返回false
            foreach (var item in sum_shop_productPriceModels)
            {
                var temp = t.Find(c => c.orderCreateTime.ToString("yyyy-MM-dd") == item.orderCreateTime.ToString("yyyy-MM-dd")
                 && c.gameId == item.gameId
                 && c.businessmobile == item.businessmobile);
                if (temp != null)
                {
                    item.id = temp.id;
                    res &= sum_Shop_ProductPriceData.Update(item);
                }
                else
                {
                    res &= sum_Shop_ProductPriceData.Insert(item) > 0;
                }
            }
            return res;

            #endregion
        }
        public static bool sumOneDay(DateTime dateTime)
        {
            return sumProcess(dateTime);
        }
        public static List<int> sumOneMonth(DateTime dateTime)
        {
            List<int> res = new List<int>();
            foreach (var item in GetDaysFromMonth(dateTime.Year, dateTime.Month))
            {
                if (sumProcess(new DateTime(dateTime.Year, dateTime.Month, item)))
                {
                    res.Add(item);
                }
            }
            return res;
        }
        private static List<int> GetDaysFromMonth(int pYear, int pMonth)
        {
            int vMax = DateTime.DaysInMonth(pYear, pMonth);
            var vItems = new List<int>();
            for (int i = 1; i < vMax; i++)
            {
                vItems.Add(i);
            }
            return vItems;
        }

    }

    public static class sharingData
    {
        public static string connectionString = "mj_cardshop_statistics";
    }
    
    public class shop_productOrderData<shop_productOrderModle> : FluentDataBase<shop_productOrderModle>
    {
        public override string connectionString => sharingData.connectionString;
        public override string tableName => "shop_productOrder";
    }
    public class shop_productOrderModle
    {
        [PrimaryKey]
        public string sysNo { get; set; }
        public string agentSysNo { get; set; }
        public DateTime createTime { get; set; }
    }

    public class shop_productOrderChildData<shop_productOrderChildModle> : FluentDataBase<shop_productOrderChildModle>
    {
        public override string connectionString => sharingData.connectionString;
        public override string tableName => "shop_productOrderChild";
    }
    public class shop_productOrderChildModle
    {
        [PrimaryKey]
        public string sysNo { get; set; }

        public string orderSysNo { get; set; }

        public string productSysNo { get; set; }

        public decimal productMemberPrice { get; set; }

        public int productCount { get; set; }

    }

    public class shop_productData<shop_productModle> : FluentDataBase<shop_productModle>
    {
        public override string connectionString => sharingData.connectionString;

        public override string tableName => "shop_product";
    }
    public class shop_productModle
    {
        [PrimaryKey]
        public string sysNo { get; set; }
        public int gameId { get; set; }
    }

    public class shop_agentData<shop_agentModle> : FluentDataBase<shop_agentModle>
    {
        public override string connectionString => sharingData.connectionString;
        public override string tableName => "shop_agent";
    }
    public class shop_agentModle
    {
        [PrimaryKey]
        public string sysNo { get; set; }

        public string businessmobile { get; set; }

    }

    public class sum_shop_productPriceData<sum_shop_productPriceModel> : FluentDataBase<sum_shop_productPriceModel>
    {
        public override string connectionString => sharingData.connectionString;
        public override string tableName => "sum_shop_productPrice";
    }
    public class sum_shop_productPriceModel
    {
        [PrimaryKey]
        public int id { get; set; }
        public string businessmobile { get; set; }
        public int gameId { get; set; }
        public decimal priceTotal { get; set; }
        public DateTime orderCreateTime { get; set; }

    }

    public static class ObjectCopy
    {
        struct Identity
        {
            int _hashcode;
            RuntimeTypeHandle _type;

            public Identity(int hashcode, RuntimeTypeHandle type)
            {
                _hashcode = hashcode;
                _type = type;
            }
        }
        //缓存对象复制的方法。  
        static Dictionary<Type, Func<object, Dictionary<Identity, object>, object>> methods1 = new Dictionary<Type, Func<object, Dictionary<Identity, object>, object>>();
        static Dictionary<Type, Action<object, Dictionary<Identity, object>, object>> methods2 = new Dictionary<Type, Action<object, Dictionary<Identity, object>, object>>();

        static List<FieldInfo> GetSettableFields(Type t)
        {
            return t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
        }

        static Func<object, Dictionary<Identity, object>, object> CreateCloneMethod1(Type type, Dictionary<Identity, object> objects)
        {
            Type tmptype;
            var fields = GetSettableFields(type);
            var dm = new DynamicMethod(string.Format("Clone{0}", Guid.NewGuid()), typeof(object), new[] { typeof(object), typeof(Dictionary<Identity, object>) }, true);
            var il = dm.GetILGenerator();
            il.DeclareLocal(type);
            il.DeclareLocal(type);
            il.DeclareLocal(typeof(Identity));
            if (!type.IsArray)
            {
                il.Emit(OpCodes.Newobj, type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null));
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldloca_S, 2);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetHashCode"));
                il.Emit(OpCodes.Ldtoken, type);
                il.Emit(OpCodes.Call, typeof(Identity).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(int), typeof(RuntimeTypeHandle) }, null));
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldloc_2);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Callvirt, typeof(Dictionary<Identity, object>).GetMethod("Add"));
                foreach (var field in fields)
                {
                    if (!field.FieldType.IsValueType && field.FieldType != typeof(String))
                    {
                        //不符合条件的字段，直接忽略，避免报错。  
                        if ((field.FieldType.IsArray && (field.FieldType.GetArrayRank() > 1 || (!(tmptype = field.FieldType.GetElementType()).IsValueType && tmptype != typeof(String) && tmptype.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null))) ||
                            (!field.FieldType.IsArray && field.FieldType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null))
                            break;
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldfld, field);
                        il.Emit(OpCodes.Ldarg_1);
                        il.EmitCall(OpCodes.Call, typeof(ObjectCopy).GetMethod("CopyImpl", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(field.FieldType), null);
                        il.Emit(OpCodes.Stfld, field);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldfld, field);
                        il.Emit(OpCodes.Stfld, field);
                    }
                }
                for (type = type.BaseType; type != null && type != typeof(object); type = type.BaseType)
                {
                    //只需要查找基类的私有成员，共有或受保护的在派生类中直接被复制过了。  
                    fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList();
                    foreach (var field in fields)
                    {
                        if (!field.FieldType.IsValueType && field.FieldType != typeof(String))
                        {
                            //不符合条件的字段，直接忽略，避免报错。  
                            if ((field.FieldType.IsArray && (field.FieldType.GetArrayRank() > 1 || (!(tmptype = field.FieldType.GetElementType()).IsValueType && tmptype != typeof(String) && tmptype.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null))) ||
                                (!field.FieldType.IsArray && field.FieldType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null))
                                break;
                            il.Emit(OpCodes.Ldloc_1);
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldfld, field);
                            il.Emit(OpCodes.Ldarg_1);
                            il.EmitCall(OpCodes.Call, typeof(ObjectCopy).GetMethod("CopyImpl", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(field.FieldType), null);
                            il.Emit(OpCodes.Stfld, field);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldloc_1);
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldfld, field);
                            il.Emit(OpCodes.Stfld, field);
                        }
                    }
                }
            }
            else
            {
                Type arraytype = type.GetElementType();
                var i = il.DeclareLocal(typeof(int));
                var lb1 = il.DefineLabel();
                var lb2 = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Stloc, i);
                il.Emit(OpCodes.Newarr, arraytype);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldloca_S, 2);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Callvirt, typeof(object).GetMethod("GetHashCode"));
                il.Emit(OpCodes.Ldtoken, type);
                il.Emit(OpCodes.Call, typeof(Identity).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(int), typeof(RuntimeTypeHandle) }, null));
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldloc_2);
                il.Emit(OpCodes.Ldloc_1);
                il.Emit(OpCodes.Callvirt, typeof(Dictionary<Identity, object>).GetMethod("Add"));
                il.Emit(OpCodes.Ldloc, i);
                il.Emit(OpCodes.Br, lb1);
                il.MarkLabel(lb2);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldloc, i);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldloc, i);
                il.Emit(OpCodes.Ldelem, arraytype);
                if (!arraytype.IsValueType && arraytype != typeof(String))
                {
                    il.EmitCall(OpCodes.Call, typeof(ObjectCopy).GetMethod("CopyImpl", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(arraytype), null);
                }
                il.Emit(OpCodes.Stelem, arraytype);
                il.Emit(OpCodes.Ldloc, i);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc, i);
                il.MarkLabel(lb1);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Clt);
                il.Emit(OpCodes.Brfalse, lb2);
            }
            il.Emit(OpCodes.Ret);


            return (Func<object, Dictionary<Identity, object>, object>)dm.CreateDelegate(typeof(Func<object, Dictionary<Identity, object>, object>));
        }

        static Action<object, Dictionary<Identity, object>, object> CreateCloneMethod2(Type type, Dictionary<Identity, object> objects)
        {
            Type tmptype;
            var fields = GetSettableFields(type);
            var dm = new DynamicMethod(string.Format("Copy{0}", Guid.NewGuid()), null, new[] { typeof(object), typeof(Dictionary<Identity, object>), typeof(object) }, true);
            var il = dm.GetILGenerator();
            il.DeclareLocal(type);
            il.DeclareLocal(type);
            il.DeclareLocal(typeof(Identity));
            if (!type.IsArray)
            {
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Stloc_0);
                foreach (var field in fields)
                {
                    if (!field.FieldType.IsValueType && field.FieldType != typeof(String))
                    {
                        //不符合条件的字段，直接忽略，避免报错。  
                        if ((field.FieldType.IsArray && (field.FieldType.GetArrayRank() > 1 || (!(tmptype = field.FieldType.GetElementType()).IsValueType && tmptype != typeof(String) && tmptype.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null))) ||
                            (!field.FieldType.IsArray && field.FieldType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null))
                            break;
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldfld, field);
                        il.Emit(OpCodes.Ldarg_1);
                        il.EmitCall(OpCodes.Call, typeof(ObjectCopy).GetMethod("CopyImpl", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(field.FieldType), null);
                        il.Emit(OpCodes.Stfld, field);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc_1);
                        il.Emit(OpCodes.Ldloc_0);
                        il.Emit(OpCodes.Ldfld, field);
                        il.Emit(OpCodes.Stfld, field);
                    }
                }
                for (type = type.BaseType; type != null && type != typeof(object); type = type.BaseType)
                {
                    //只需要查找基类的私有成员，共有或受保护的在派生类中直接被复制过了。  
                    fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).ToList();
                    foreach (var field in fields)
                    {
                        if (!field.FieldType.IsValueType && field.FieldType != typeof(String))
                        {
                            //不符合条件的字段，直接忽略，避免报错。  
                            if ((field.FieldType.IsArray && (field.FieldType.GetArrayRank() > 1 || (!(tmptype = field.FieldType.GetElementType()).IsValueType && tmptype != typeof(String) && tmptype.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null))) ||
                                (!field.FieldType.IsArray && field.FieldType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null))
                                break;
                            il.Emit(OpCodes.Ldloc_1);
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldfld, field);
                            il.Emit(OpCodes.Ldarg_1);
                            il.EmitCall(OpCodes.Call, typeof(ObjectCopy).GetMethod("CopyImpl", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(field.FieldType), null);
                            il.Emit(OpCodes.Stfld, field);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldloc_1);
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Ldfld, field);
                            il.Emit(OpCodes.Stfld, field);
                        }
                    }
                }
            }
            else
            {
                Type arraytype = type.GetElementType();
                var i = il.DeclareLocal(typeof(int));
                var lb1 = il.DefineLabel();
                var lb2 = il.DefineLabel();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Stloc, i);
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Castclass, type);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc_1);
                il.Emit(OpCodes.Ldloc, i);
                il.Emit(OpCodes.Br, lb1);
                il.MarkLabel(lb2);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldloc, i);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldloc, i);
                il.Emit(OpCodes.Ldelem, arraytype);
                if (!arraytype.IsValueType && arraytype != typeof(String))
                {
                    il.EmitCall(OpCodes.Call, typeof(ObjectCopy).GetMethod("CopyImpl", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(arraytype), null);
                }
                il.Emit(OpCodes.Stelem, arraytype);
                il.Emit(OpCodes.Ldloc, i);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Stloc, i);
                il.MarkLabel(lb1);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Clt);
                il.Emit(OpCodes.Brfalse, lb2);
            }
            il.Emit(OpCodes.Ret);


            return (Action<object, Dictionary<Identity, object>, object>)dm.CreateDelegate(typeof(Action<object, Dictionary<Identity, object>, object>));
        }

        static T CopyImpl<T>(T source, Dictionary<Identity, object> objects) where T : class
        {
            //为空则直接返回null  
            if (source == null)
                return null;

            Type type = source.GetType();
            Identity id = new Identity(source.GetHashCode(), type.TypeHandle);
            object result;
            //如果发现曾经复制过，用之前的，从而停止递归复制。  
            if (!objects.TryGetValue(id, out result))
            {
                //最后查找对象的复制方法，如果不存在，创建新的。  
                Func<object, Dictionary<Identity, object>, object> method;
                if (!methods1.TryGetValue(type, out method))
                {
                    method = CreateCloneMethod1(type, objects);
                    methods1.Add(type, method);
                }
                result = method(source, objects);
            }
            return (T)result;
        }


        /// <summary>  
        /// 创建对象深度复制的副本  
        /// </summary>  
        public static T ToObjectCopy<T>(this T source) where T : class
        {
            Type type = source.GetType();
            Dictionary<Identity, object> objects = new Dictionary<Identity, object>();//存放内嵌引用类型的复制链，避免构成一个环。  
            Func<object, Dictionary<Identity, object>, object> method;
            if (!methods1.TryGetValue(type, out method))
            {
                method = CreateCloneMethod1(type, objects);
                methods1.Add(type, method);
            }
            return (T)method(source, objects);
        }


        /// <summary>  
        /// 将source对象的所有属性复制到target对象中，深度复制  
        /// </summary>  
        public static void ObjectCopyTo<T>(this T source, T target) where T : class
        {
            if (target == null)
                throw new Exception("将要复制的目标未初始化");
            Type type = source.GetType();
            if (type != target.GetType())
                throw new Exception("要复制的对象类型不同，无法复制");
            Dictionary<Identity, object> objects = new Dictionary<Identity, object>();//存放内嵌引用类型的复制链，避免构成一个环。  
            objects.Add(new Identity(source.GetHashCode(), type.TypeHandle), source);
            Action<object, Dictionary<Identity, object>, object> method;
            if (!methods2.TryGetValue(type, out method))
            {
                method = CreateCloneMethod2(type, objects);
                methods2.Add(type, method);
            }
            method(source, objects, target);
        }
    }



}
