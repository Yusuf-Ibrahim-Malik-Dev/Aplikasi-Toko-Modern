
using System;
using System.Collections.Generic;
using System.Data;
using CommonClass;

namespace DAAccess
{
    public class DABarang
    {
        // =========================================================
        // SUBMIT BARANG
        // Stored Procedure: proc_baranginsertupdate
        // =========================================================
        public string submitBarang(ClassBarang data)
        {
            try
            {
                string msg = string.Empty;

                using (DbConnection conn = new DbConnection(dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[5]
                    {
                        data.kodebarang,
                        data.namabarang,
                        data.hargasatuan,
                        data.satuan,
                        data.stok
                    };

                    conn.ExecReader("proc_baranginsertupdate @0, @1, @2, @3, @4", par);

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("pesan");
                    }
                }

                return msg;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // BIND LIST BARANG
        // Stored Procedure: proc_getAllData
        // =========================================================
        public List<ClassBarang> BindList()
        {
            List<ClassBarang> list = new List<ClassBarang>();

            using (DbConnection conn = new DbConnection(
                dbConfiguration.createConection().ConnectionString))
            {
                conn.ExecReader("EXEC dbo.proc_getAllData");

                while (conn.HasRow())
                {
                    ClassBarang data = new ClassBarang();

                    data.kodebarang = conn.GetFieldValue("kodebarang");
                    data.namabarang = conn.GetFieldValue("namabarang");
                    data.hargasatuan = conn.GetFieldValue<decimal>("hargasatuan");
                    data.satuan = conn.GetFieldValue("satuan");
                    data.stok = conn.GetFieldValue<int>("stok");

                    list.Add(data);
                }
            }

            return list;
        }

        // =========================================================
        // DELETE BARANG
        // Stored Procedure: proc_deletebarang
        // =========================================================
        public ClassBarang getdeletebarang(string kodebarang)
        {
            try
            {
                ClassBarang data = new ClassBarang();

                using (DbConnection conn = new DbConnection(dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[1]
                    {
                        kodebarang
                    };

                    conn.ExecReader(
                        "proc_deletebarang @0",
                        par
                    );

                    while (conn.HasRow())
                    {
                        data.kodebarang =
                            conn.GetFieldValue("kodebarang");

                        data.namabarang =
                            conn.GetFieldValue("namabarang");

                        data.hargasatuan =
                            conn.GetFieldValue<decimal>("hargasatuan");

                        data.satuan =
                            conn.GetFieldValue("satuan");
                    }
                }

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
