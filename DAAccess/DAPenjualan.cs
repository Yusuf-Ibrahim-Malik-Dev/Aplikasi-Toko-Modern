
using System;
using System.Collections.Generic;
using CommonClass;

namespace DAAccess
{
    public class DAPenjualan
    {
        // =========================================================
        // GENERATE NOMOR FAKTUR
        // Stored Procedure: proc_generatefakturno
        // =========================================================
        public string generatenofaktur()
        {
            string msg = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    conn.ExecReader("proc_generatefakturno");

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("hasil");
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
        // INSERT TEMPORARY ISI FAKTUR
        // Stored Procedure: proc_inserttempisifaktur
        // =========================================================
        public string insterttemp(ClassTempIsiFaktur obj)
        {
            string keluaran = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[6]
                    {
                        obj.nofaktur,
                        obj.kodebarang,
                        obj.namabarang,
                        obj.hargasatuan,
                        obj.jumlahbeli,
                        obj.total
                    };

                    conn.ExecReader(
                        "proc_inserttempisifaktur @0, @1, @2, @3, @4, @5",
                        par
                    );

                    while (conn.HasRow())
                    {
                        keluaran = conn.GetFieldValue("hasil");
                    }
                }

                return keluaran;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // GET DATA BARANG BERDASARKAN KODE
        // Stored Procedure: proc_getdatabykode
        // =========================================================
        public ClassBarang getdatabarangbykode(string kodebarang)
        {
            ClassBarang data = new ClassBarang();

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[1]
                    {
                        kodebarang
                    };

                    conn.ExecReader(
                        "proc_getdatabykode @0",
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

        // =========================================================
        // SUBMIT FAKTUR
        // Stored Procedure: proc_insertfaktur
        // =========================================================
        public string submitfaktur(ClassFaktur obj)
        {
            string msg = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[2]
                    {
                        obj.nofaktur,
                        obj.total
                    };

                    conn.ExecReader(
                        "proc_insertfaktur @0, @1",
                        par
                    );

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("hasil");
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
        // SUBMIT ISI FAKTUR
        // Stored Procedure: proc_insertisifaktur
        // =========================================================
        public string submitisifaktur(ClassIsiFaktur obj)
        {
            string msg = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[6]
                    {
                        obj.nofaktur,
                        obj.kodebarang,
                        obj.namabarang,
                        obj.hargasatuan,
                        obj.jumlah,
                        obj.total
                    };

                    conn.ExecReader(
                        "proc_insertisifaktur @0, @1, @2, @3, @4, @5",
                        par
                    );

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("hasil");
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
        // DELETE TEMPORARY FAKTUR
        // Stored Procedure: proc_deleteFakturTemporary
        // =========================================================
        public string deletetemporary()
        {
            string msg = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    conn.ExecReader("proc_deleteFakturTemporary");

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("hasil");
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
        // GET ISI GRID FAKTUR
        // Stored Procedure: proc_getisigrid
        // =========================================================
        public List<ClassTempIsiFaktur> getisigrid(string nofaktur)
        {
            List<ClassTempIsiFaktur> list =
                new List<ClassTempIsiFaktur>();

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[1]
                    {
                        nofaktur
                    };

                    conn.ExecReader(
                        "proc_getisigrid @0",
                        par
                    );

                    while (conn.HasRow())
                    {
                        ClassTempIsiFaktur data =
                            new ClassTempIsiFaktur();

                        data.nofaktur =
                            conn.GetFieldValue("nofaktur");

                        data.kodebarang =
                            conn.GetFieldValue("kodebarang");

                        data.namabarang =
                            conn.GetFieldValue("namabarang");

                        data.hargasatuan =
                            conn.GetFieldValue<decimal>("hargasatuan");

                        data.jumlahbeli =
                            conn.GetFieldValue<short>("jumlahbeli");

                        data.total =
                            conn.GetFieldValue<decimal>("total");

                        list.Add(data);
                    }
                }

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // =========================================================
        // SUBMIT FAKTUR JUMLAH PEMBAYARAN
        // Stored Procedure: proc_insertfakturjumlah
        // =========================================================
        public string submitfakturjumlah(ClassIsiFakturJumlah obj)
        {
            string msg = string.Empty;

            try
            {
                using (DbConnection conn =
                    new DbConnection(
                        dbConfiguration.createConection().ConnectionString))
                {
                    object[] par = new object[3]
                    {
                        obj.nofaktur,
                        obj.jumlahuang,
                        obj.kembali
                    };

                    conn.ExecReader(
                        "exec proc_insertfakturjumlah @0, @1, @2",
                        par
                    );

                    while (conn.HasRow())
                    {
                        msg = conn.GetFieldValue("hasil");
                    }
                }

                return msg;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
