﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using Nancy.Rest.Annotations.Atributes;
using Nancy.Rest.Client.ContractResolver;
using Nancy.Rest.Client.Helpers;
using Nancy.Rest.Client.Rest;
using Newtonsoft.Json;
using ParameterType = Nancy.Rest.Client.Helpers.ParameterType;

namespace Nancy.Rest.Client
{
    public class ClientFactory
    {

        public static T Create<T>(string path, Dictionary<Type, Type> deserializationmappings=null, string defaultlevelqueryparametername="level", string defaultexcludtagsqueryparametername="excludetags", int defaulttimeoutinseconds=60) where T : class
        {
            List<RestBasePath> paths = typeof(T).GetCustomAttributesFromInterfaces<RestBasePath>().ToList();
            if (paths.Count > 0)
            {
                string s = paths[0].BasePath;
                if (path.EndsWith("/"))
                    path = path.Substring(0, path.Length - 1);
                if (s.StartsWith("/"))
                    s = s.Substring(1);
                path = path + "/" + s;
            }
            if (!path.EndsWith("/"))
                path = path + "/";
            return Create<T>(path, int.MaxValue, null, deserializationmappings,defaultlevelqueryparametername, defaultexcludtagsqueryparametername,defaulttimeoutinseconds);
        }
        private static T Create<T>(string path, int level, IEnumerable<string> tags, Dictionary<Type, Type> deserializationmappings, string defaultlevelqueryparametername, string defaultexcludtagsqueryparametername, int defaulttimeoutinseconds,bool filter=true) where T: class
        {
            dynamic dexp = new ExpandoObject();

            IDictionary<string, object> exp = (IDictionary<string, object>) dexp;
            dexp.DYN_defaultlevelqueryparametername = defaultlevelqueryparametername;
            dexp.DYN_defaultexcludtagsqueryparametername = defaultexcludtagsqueryparametername;
            dexp.DYN_level = level;
            dexp.DYN_tags = tags;
            dexp.DYN_deserializationmappings = deserializationmappings;
            dexp.DYN_defaulttimeoutinseconds = defaulttimeoutinseconds;
            if (filter && deserializationmappings != null)
            {
                foreach (Type t in deserializationmappings.Keys)
                {
                    if (!t.IsAssignableFrom(deserializationmappings[t]))
                    {
                        throw new ArgumentException("The mapping type '"+deserializationmappings[t].Name +"' is not child of '"+t.Name+"'");
                    }
                }
            }
            bool hasfilterinterface = (typeof(T).GetInterfaces().Any(a => a.Name == typeof(Interfaces.IFilter<>).Name));
            List<Type> ifaces=new List<Type>() { typeof(T)};
            ifaces.AddRange(typeof(T).GetInterfaces());
            foreach (MethodInfo m in ifaces.SelectMany(a=>a.GetMethods()))
            {
                List<Annotations.Atributes.Rest> rests = m.GetCustomAttributes<Annotations.Atributes.Rest>().ToList();
                if (rests.Count > 0)
                {
                    MethodDefinition defs = new MethodDefinition();
                    defs.RestAttribute = rests[0];
                    defs.BasePath = path;
                    defs.Parameters = m.GetParameters().Select(a => new Tuple<string, Type>(a.Name, a.ParameterType)).ToList();
                    defs.ReturnType = m.ReturnType;
                    if (hasfilterinterface && (m.Name == "FilterWithLevel" || m.Name== "FilterWithTags" || m.Name== "FilterWithLevelAndTags"))
                        continue;

                    if (m.IsAsyncMethod())
                    {

                        switch (defs.Parameters.Count)
                        {
                            case 0:
                                exp[m.Name] = Return<dynamic>.Arguments(() => DoAsyncClient(dexp, defs));
                                break;
                            case 1:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic>((a) => DoAsyncClient(dexp, defs, a));
                                break;
                            case 2:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic>((a, b) => DoAsyncClient(dexp, defs, a, b));
                                break;
                            case 3:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic>((a, b, c) => DoAsyncClient(dexp, defs, a, b, c));
                                break;
                            case 4:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic>((a, b, c, d) => DoAsyncClient(dexp, defs, a, b, c, d));
                                break;
                            case 5:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e) => DoAsyncClient(dexp, defs, a, b, c, d, e));
                                break;
                            case 6:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f) => DoAsyncClient(dexp, defs, a, b, c, d, e, f));
                                break;
                            case 7:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f, g) => DoAsyncClient(dexp, defs, a, b, c, d, e, f, g));
                                break;
                            case 8:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f, g, h) => DoAsyncClient(dexp, defs, a, b, c, d, e, f, g, h));
                                break;
                            default:
                                throw new NotImplementedException("It only support till 8 parameters feel free to add more here :O");
                        }
                    }
                    else
                    {
                        switch (defs.Parameters.Count)
                        {
                            case 0:
                                exp[m.Name] = Return<dynamic>.Arguments(() => DoSyncClient(dexp, defs));
                                break;
                            case 1:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic>((a) => DoSyncClient(dexp, defs, a));
                                break;
                            case 2:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic>((a, b) => DoSyncClient(dexp, defs, a, b));
                                break;
                            case 3:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic>((a, b, c) => DoSyncClient(dexp, defs, a, b, c));
                                break;
                            case 4:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic>((a, b, c, d) => DoSyncClient(dexp, defs, a, b, c, d));
                                break;
                            case 5:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e) => DoSyncClient(dexp, defs, a, b, c, d, e));
                                break;
                            case 6:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f) => DoSyncClient(dexp, defs, a, b, c, d, e, f));
                                break;
                            case 7:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f, g) => DoSyncClient(dexp, defs, a, b, c, d, e, f, g));
                                break;
                            case 8:
                                exp[m.Name] = Return<dynamic>.Arguments<dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic, dynamic>((a, b, c, d, e, f, g, h) => DoSyncClient(dexp, defs, a, b, c, d, e, f, g, h));
                                break;
                            default:
                                throw new NotImplementedException("It only support till 8 parameters feel free to add more here :O");
                        }                    
                    }
                }
            }
            T inter = Impromptu.ActLike<T>(dexp);
            if (hasfilterinterface)
            {
                if (filter)
                {
                    exp["FilterWithLevel"] = Return<T>.Arguments<int>((a) => Create<T>(path, a, null, deserializationmappings, defaultlevelqueryparametername, defaultexcludtagsqueryparametername, defaulttimeoutinseconds, false));
                    exp["FilterWithTags"] = Return<T>.Arguments<IEnumerable<string>>((a) => Create<T>(path, int.MaxValue, a, deserializationmappings, defaultlevelqueryparametername, defaultexcludtagsqueryparametername, defaulttimeoutinseconds, false));
                    exp["FilterWithLevelAndTags"] = Return<T>.Arguments<int, IEnumerable<string>>((a, b) => Create<T>(path, a, b, deserializationmappings, defaultlevelqueryparametername, defaultexcludtagsqueryparametername, defaulttimeoutinseconds, false));
                }
                else
                {
                    exp["FilterWithLevel"] = Return<T>.Arguments<int>((a) => inter);
                    exp["FilterWithTags"] = Return<T>.Arguments<IEnumerable<string>>((a) => inter);
                    exp["FilterWithLevelAndTags"] = Return<T>.Arguments<int, IEnumerable<string>>((a, b) => inter);
                }
            }
            return inter;
        }

        private static dynamic DoSyncClient(dynamic dexp, MethodDefinition def, params dynamic[] parameters)
        {
            Request req = CreateRequest(dexp, def, parameters);
            return Task.Run(async () => await SmallWebClient.RestRequest(req)).Result;
        }
        private static async Task<dynamic> DoAsyncClient(dynamic dexp, MethodDefinition def, params dynamic[] parameters)
        {
            Request req = CreateRequest(dexp, def, parameters);
            return await SmallWebClient.RestRequest(req);
        }

        private static Request CreateRequest(dynamic dexp, MethodDefinition def, dynamic[] parameters)
        {
            string defaultlevelqueryparametername = dexp.DYN_defaultlevelqueryparametername;
            string defaultexcludtagsqueryparametername = dexp.DYN_defaultexcludtagsqueryparametername;
            int level = dexp.DYN_level;
            List<string> tags = dexp.DYN_tags;
            Request request = ProcessPath(def.RestAttribute.Route, def, parameters);
            if (level != int.MaxValue)
                request.AddQueryParamater(defaultlevelqueryparametername, level.ToString());
            if (tags != null && tags.Count > 0)
                request.AddQueryParamater(defaultexcludtagsqueryparametername, string.Join(",", tags));
            request.SerializerSettings = new JsonSerializerSettings{ReferenceLoopHandling = ReferenceLoopHandling.Serialize};;
            if (dexp.DYN_deserializationmappings != null)
                request.SerializerSettings.ContractResolver = new MappedContractResolver((Dictionary<Type, Type>)dexp.DYN_deserializationmappings);
            request.Timeout = TimeSpan.FromSeconds(dexp.DYN_defaulttimeoutinseconds);
            return request;
        }

        private static Regex rpath=new Regex("\\{(.*?)\\}",RegexOptions.Compiled);
        private static Regex options = new Regex("\\((.*?)\\)", RegexOptions.Compiled);

        //TODO It only support Nancy constrains except version and optional parameters, others types should be added.

        private static Request ProcessPath(string path, MethodDefinition def, dynamic[] parameters)
        {
            List<Parameter> pars=new List<Parameter>();
            MatchCollection collection = rpath.Matches(path);
            foreach (Match m in collection)
            {
                if (m.Success)
                {
                    string value = m.Groups[1].Value;
                    Parameter p = new Parameter();
                    p.Original = value;
                    bool optional = false;
                    string constraint = null;
                    string ops = null;
                    int idx = value.LastIndexOf('?');
                    if (idx > 0)
                    {
                        value = value.Substring(0, idx);
                        optional = true;
                    }
                    idx = value.LastIndexOf(':');
                    if (idx >= 0)
                    {
                        constraint = value.Substring(idx + 1);
                        Match optmatch = options.Match(constraint);
                        if (optmatch.Success)
                        {
                            ops = optmatch.Groups[1].Value;
                            constraint = constraint.Substring(0, optmatch.Groups[1].Index);
                        }
                        value = value.Substring(0, idx);
                    }
                    Tuple<string, Type> tx = def.Parameters.FirstOrDefault(a => a.Item1 == value);
                    if (tx == null)
                        throw new Exception("Unable to find parameter '" + value + "' in method with route : " + def.RestAttribute.Route);
                    p.Name = tx.Item1;
                    dynamic par = parameters[def.Parameters.IndexOf(tx)];
                    if (par == null && optional)
                        p.Value = string.Empty;
                    else if (constraint != null)
                    {
                        ParameterType type = ParameterType.InstanceTypes.FirstOrDefault(a => a.Name == constraint);
                        if (type==null)
                            throw new Exception("Invalid Contraint: " + constraint);
                        ParameterResult res = type.Convert((object) par, ops, p.Name, optional);
                        if (!res.Success)
                            throw new Exception(res.Error);
                        p.Value = res.Value;
                    }
                    else
                    {
                        if (par is DateTime)
                        {
                            p.Value = ((DateTime) par).ToString("o",CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            TypeConverter c = TypeDescriptor.GetConverter(par.GetType());
                            if (c.CanConvertTo(typeof(string)))
                                p.Value = c.ConvertToInvariantString(par);
                            else
                                throw new Exception("Unable to convert parameter '" + value + "' to string");
                        }
  
                    }
                    pars.Add(p);
                }
            }
            foreach (Parameter p in pars)
            {
                path = path.Replace("{" + p.Original + "}", p.Value);
            }
            List<string> names = pars.Select(a => a.Name).ToList();
            List<int> bodyitems = def.Parameters.Where(a => !names.Contains(a.Item1)).Select(a => def.Parameters.IndexOf(a)).ToList();
            object body = null;
            if (bodyitems.Count > 1)
            {
                Dictionary<string, object> bjson=new Dictionary<string, object>();
                foreach(int p in bodyitems)
                    bjson.Add(def.Parameters[p].Item1,parameters[p]);
                body = bjson;
            }
            else if (bodyitems.Count == 1)
                body = parameters[bodyitems[0]];
            Request r=new Request();
            r.Path = path;
            r.BaseUri = new Uri(def.BasePath);
            r.BodyObject = body;
            r.Method = def.RestAttribute.Verb.ToHttpMethod();
            r.ReturnType = def.ReturnType;
            return r;
        }
    }
}
