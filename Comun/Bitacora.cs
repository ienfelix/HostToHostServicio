using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Comun
{
    public class Bitacora
    {
        private String _carpetaBitacora = String.Empty;

        public Bitacora()
        {
            _carpetaBitacora = ConfigurationManager.AppSettings[Constante.CARPETA_BITACORA] ?? String.Empty;
        }

        public async Task RegistrarEventoAsync(CancellationToken cancelToken, String tipo, String proyecto, String clase, String metodo, String mensaje)
        {
            String rutaArchivo = String.Empty;
            try
            {
                String fecha = DateTime.Now.ToString("yyyyMMdd");
                String nombreArchivo = String.Format("{0}_{1}{2}", Constante.NOMBRE_BITACORA, fecha, Constante.EXTENSION_TXT);
                rutaArchivo = String.Format("{0}{1}", _carpetaBitacora, nombreArchivo);
                fecha = String.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                String texto = String.Format("{0} {1} > {2} - {3} - {4} - {5}{6}", tipo, fecha, proyecto, clase, metodo, mensaje, Environment.NewLine);

                if (!Directory.Exists(_carpetaBitacora))
                {
                    Directory.CreateDirectory(_carpetaBitacora);
                }

                using (FileStream fileStream = File.Open(rutaArchivo, FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    Encoding ISO = Encoding.GetEncoding(Constante.ISO_8859_1);
                    Encoding UTF8 = Encoding.UTF8;
                    byte[] bytes = Encoding.UTF8.GetBytes(texto);
                    byte[] isoBytes = Encoding.Convert(UTF8, ISO, bytes);
                    await fileStream.WriteAsync(isoBytes, Constante._0, isoBytes.Length, cancelToken);
                }
            }
            catch (Exception e)
            {
                File.WriteAllText(rutaArchivo, e.Message);
                throw e;
            }
        }

        public async Task RegistrarEventoAsync(CancellationToken cancelToken, String tipo, String proyecto, String clase, String metodo, String mensaje, String excepcion)
        {
            String rutaArchivo = String.Empty;
            try
            {
                String fecha = DateTime.Now.ToString("yyyyMMdd");
                String nombreArchivo = String.Format("{0}_{1}{2}", Constante.NOMBRE_BITACORA, fecha, Constante.EXTENSION_TXT);
                rutaArchivo = String.Format("{0}{1}", _carpetaBitacora, nombreArchivo);
                fecha = String.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                String texto = String.Format("{0} {1} > {2} - {3} - {4} - {5} | {6}{7}", tipo, fecha, proyecto, clase, metodo, mensaje, excepcion, Environment.NewLine);

                if (!Directory.Exists(_carpetaBitacora))
                {
                    Directory.CreateDirectory(_carpetaBitacora);
                }

                using (FileStream fileStream = File.Open(rutaArchivo, FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    Encoding ISO = Encoding.GetEncoding(Constante.ISO_8859_1);
                    Encoding UTF8 = Encoding.UTF8;
                    byte[] bytes = Encoding.UTF8.GetBytes(texto);
                    byte[] isoBytes = Encoding.Convert(UTF8, ISO, bytes);
                    await fileStream.WriteAsync(isoBytes, Constante._0, isoBytes.Length, cancelToken);
                }
            }
            catch (Exception e)
            {
                File.WriteAllText(rutaArchivo, e.Message);
                throw e;
            }
        }
    }
}
