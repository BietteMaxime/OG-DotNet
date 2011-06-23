//-----------------------------------------------------------------------
// <copyright file="JavaException.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using Fudge;
using OGDotNet.Mappedtypes.engine.View.permission;

namespace OGDotNet.Mappedtypes.engine.View.listener
{
    public class JavaException
    {
        private readonly string _type;
        private readonly string _message;

        public JavaException(string type)
        {
            _type = type;
        }

        public JavaException(string type, string message)
        {
            _type = type;
            _message = message;
        }

        public string Type
        {
            get { return _type; }
        }

        public string Message
        {
            get { return _message; }
        }

        public override string ToString()
        {
            return string.Format("[JavaException {0}: {1}]", Type, Message);
        }

        private static readonly IDictionary<string, Type> DotNetTypesByJavaTypeName = new Dictionary<string, Type>
                                                                                          {
                                                                                              {"java.lang.IllegalArgumentException", typeof(ArgumentException)},
                                                                                              {"java.lang.NullPointerException", typeof(NullReferenceException)},
                                                                                              {"java.lang.IllegalStateException", typeof(InvalidOperationException)},
                                                                                              {"com.opengamma.engine.view.permission.ViewPermissionException", typeof(ViewPermissionException)},
                                                                                              {"com.opengamma.OpenGammaRuntimeException", typeof(OpenGammaException)},
                                                                                              {"com.opengamma.DataNotFoundException", typeof(DataNotFoundException)},
                                                                                              {"org.fudgemsg.FudgeRuntimeException", typeof(FudgeRuntimeException)}
                                                                                          };

        public Exception BuildException()
        {
            Type exceptionType;
            if (DotNetTypesByJavaTypeName.TryGetValue(this.Type, out exceptionType))
            {
                if (this.Message == null)
                {
                    ConstructorInfo constructorInfo = exceptionType.GetConstructor(new Type[] { });
                    if (constructorInfo == null)
                        throw new ArgumentException(string.Format("Can't construct exception type {0}->{1}", this.Type, exceptionType), "javaType");

                    return (Exception)constructorInfo.Invoke(new object[] { });
                }
                else
                {
                    ConstructorInfo constructorInfo = exceptionType.GetConstructor(new[] { typeof(string) });

                    if (constructorInfo == null)
                        throw new ArgumentException(string.Format("Can't construct exception type {0}->{1}", this.Type, exceptionType), "javaType");
                    if (constructorInfo.GetParameters()[0].Name != "message")
                        throw new ArgumentException(string.Format("Exception type {0}->{1} expectes {2} not message", this.Type, exceptionType, this.Message), "javaType");
                    return (Exception)constructorInfo.Invoke(new object[] { this.Message });
                }
            }
            else
            {
                return BuildGenericException(this.Type, this.Message);
            }
        }

        private static Exception BuildGenericException(string javaType, string message = null)
        {
            return new Exception(string.Format("{0}: {1} - {2}", javaType, message, "Unmappable java exception ocurred"));
        }
    }
}