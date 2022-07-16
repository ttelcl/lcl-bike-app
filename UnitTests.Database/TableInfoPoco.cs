/*
 * (c) 2022  ttelcl / ttelcl
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Database
{
  /// <summary>
  /// An immutable POCO class modeling the standard [INFORMATION_SCHEMA].[TABLES]
  /// table content
  /// </summary>
  public class TableInfoPoco
  {
    public TableInfoPoco(
      string catalog, string schema, string name)
    {
      Catalog=catalog;
      Schema=schema;
      Name=name;
    }

    public string Catalog { get; }

    public string Schema { get; }

    public string Name { get; }

  }

}
