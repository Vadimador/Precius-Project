using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;

namespace connector_dev
{
    class Program
    {
        

        //private static IntPtr hport = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr))); // le handler du port

        static private SafeFileHandle portHandle = null;

        /// <summary>
        /// Windows return code for successful operations.
        /// </summary>
        private const uint OK = 0;

        /// <summary>
        /// The communication port name for this client.
        /// </summary>
        static private string portName = "\\\\preciusMF"; // le nom du minifilter


        /// <summary>
        /// Opens a new connection to a communication server port that is created by a file system MiniFilter.
        /// </summary>
        /// <param name="portName">
        /// NULL-terminated wide-character string containing the fully qualified name of the communication server port (for example, <c>"\\MyFilterPort"</c>).
        /// </param>
        /// <param name="options">
        /// Currently unused.
        /// Callers should set this parameter to zero.
        /// </param>
        /// <param name="context">
        /// Pointer to caller-supplied context information to be passed to the kernel-mode MiniFilter's connect notification routine.
        /// This parameter is optional and can be <see cref="IntPtr.Zero"/>.
        /// </param>
        /// <param name="sizeOfContext">
        /// Size, in bytes, of the structure that the <paramref name="context"/> parameter points to.
        /// If the value of <paramref name="context"/> is non-<see cref="IntPtr.Zero"/>, this parameter must be nonzero.
        /// If <paramref name="context"/> is <see cref="IntPtr.Zero"/>, this parameter must be zero.
        /// </param>
        /// <param name="securityAttributes">
        /// Pointer to a <c>SECURITY_ATTRIBUTES</c> structure that determines whether the returned handle can be inherited by child processes.
        /// This parameter is optional and can be <see cref="IntPtr.Zero"/>. If this parameter is <see cref="IntPtr.Zero"/>, the handle cannot be inherited.
        /// </param>
        /// <param name="portHandle">
        /// Pointer to a caller-allocated variable that receives a handle for the newly created connection port if the call to 
        /// <c>FilterConnectCommunicationPort</c> succeeds; otherwise, it receives <c>INVALID_HANDLE_VALUE</c>.
        /// </param>
        /// <returns>
        /// <see cref="Ok"/> if successful. Otherwise, it returns an error value.
        /// </returns>
        [DllImport("fltlib.dll")]
        private static extern uint FilterConnectCommunicationPort(
            /* [in]  */ [MarshalAs(UnmanagedType.LPWStr)] string portName,
            /* [in]  */ uint options,
            /* [in]  */ IntPtr context,
            /* [in]  */ short sizeOfContext,
            /* [in]  */ IntPtr securityAttributes,
            /* [out] */ out SafeFileHandle portHandle);

        /// <summary>
        /// Gets a message from a kernel-mode MiniFilter.
        /// </summary>
        /// <param name="portHandle">
        /// Communication port handle returned by a previous call to <see cref="FilterConnectCommunicationPort"/>.
        /// This parameter is required and cannot be <see cref="IntPtr.Zero"/>.
        /// </param>
        /// <param name="messageBuffer">
        /// Pointer to a caller-allocated buffer that receives the message from the MiniFilter.
        /// </param>
        /// <param name="messageBufferSize">
        /// Size, in bytes, of the buffer that the <paramref name="messageBuffer"/> parameter points to.
        /// </param>
        /// <param name="overlapped">
        /// Pointer to an <see cref="NativeOverlapped"/> structure.
        /// </param>
        /// <returns>
        /// <see cref="Ok"/> if successful. Otherwise, it returns an error value.
        /// </returns>
        [DllImport("fltlib.dll")]
        private static extern uint FilterGetMessage(
            /* [in]  */ SafeFileHandle portHandle,
            /* [out] */ IntPtr messageBuffer,
            /* [in]  */ int messageBufferSize,
            /* [out] */ ref NativeOverlapped overlapped);

        /// <summary>
        /// Sends a message to a kernel-mode MiniFilter.
        /// </summary>
        /// <param name="portHandle">
        /// Communication port handle returned by a previous call to <see cref="FilterConnectCommunicationPort"/>.
        /// This parameter is required and cannot be <see cref="IntPtr.Zero"/>.
        /// </param>
        /// <param name="inBuffer">
        /// Pointer to a caller-allocated buffer containing the message to be sent to the MiniFilter.
        /// The message format is caller-defined.
        /// This parameter is required and cannot be <see cref="IntPtr.Zero"/>.
        /// </param>
        /// <param name="inBufferSize">
        /// Size, in bytes, of the buffer pointed to by <paramref name="inBuffer"/>.
        /// </param>
        /// <param name="outBuffer">
        /// Pointer to a caller-allocated buffer that receives the reply (if any) from the MiniFilter.
        /// </param>
        /// <param name="outBufferSize">
        /// Size, in bytes, of the buffer pointed to by <paramref name="outBuffer"/>.
        /// This value is ignored if <paramref name="outBuffer"/> is <see cref="IntPtr.Zero"/>.
        /// </param>
        /// <param name="bytesReturned">
        /// Variable that receives the number of bytes returned in the buffer that
        /// <paramref name="outBuffer"/> points to if the call to <c>FilterSendMessage</c> succeeds.
        /// This parameter is required.
        /// </param>
        /// <returns>
        /// <see cref="Ok"/> if successful. Otherwise, it returns an error value.
        /// </returns>
        [DllImport("fltlib.dll")]
        private static extern uint FilterSendMessage(
            /* [in]  */ SafeFileHandle portHandle,
            /* [in]  */ IntPtr inBuffer,
            /* [in]  */ uint inBufferSize,
            /* [out] */ IntPtr outBuffer,
            /* [in]  */ uint outBufferSize,
            /* [out] */ out uint bytesReturned);

        static void Main(string[] args)
        {
            Console.WriteLine("Début du test de connexion");

            try
            {
                Connect();
                readMessage();
                Disconnect();
            }
            catch(Exception e)
            {
                Console.WriteLine("/!\\ - An error have appear : \n" + e.ToString() + "\n");
            }
            finally
            {
                if(portHandle != null)
                {
                    portHandle.Dispose();
                    portHandle = null;
                }
            }
        }

        public static uint Connect()
        {
            uint status = 1;
            if (portHandle == null)
            {

                Console.WriteLine("[!] Tentative de connexion.");
                status = FilterConnectCommunicationPort(portName, 0, IntPtr.Zero, 0, IntPtr.Zero, out portHandle);

                if (status == OK)
                {
                    Console.WriteLine("[+] Tentative réussi !");
                    // on fait nos bails

                }
                else
                {
                    portHandle = null;
                    throw new Exception("[-] La connexion n'a pas fonctionné Error : HRESULT 0x" + status.ToString("X") + "\n" + Marshal.GetExceptionForHR((int)status).Message);
                }
            }
            else
            {
                Console.WriteLine("[-] Le client est déjà connecté, reconnexion impossible");
            }
            return status;
        }

        public static void readMessage()
        {
            if (portHandle != null)
            {
                int responseSize = 260 + "filescan|".Length; // la taille maximal d'un chemin de fichier par défaut est de 260 caractère plus le nom du signal.
                IntPtr response = IntPtr.Zero; // la réponse du minifilter, il faut Marshall
                IntPtr message = IntPtr.Zero; // le message de questionnement
                uint bytesReceived; // jsp wtf
                try
                {

                    Console.WriteLine("[!] Tentative d'envoie de message au minifilter.");
                    response = Marshal.AllocHGlobal(responseSize);
                    message = Marshal.AllocHGlobal(1); // on envoie juste un bip
                    
                    Console.WriteLine("[+] Allouement des buffer réussi !");

                    uint status = FilterSendMessage(portHandle, message, 1, response, (uint)responseSize, out bytesReceived);
                    if (status == OK)
                    {
                        Console.WriteLine("[+] Message envoyé avec succés !!!!!");
                        // lessssssss gooooooooooooo
                    }
                    else
                    {
                        throw new Exception("[-] La connexion n'a pas fonctionné Error : HRESULT 0x" + status.ToString("X") + "\n" + Marshal.GetExceptionForHR((int)status).Message);
                    }

                }
                finally
                {
                    Console.WriteLine("[+] Nettoyage de l'allouement des buffers");
                    Marshal.FreeHGlobal(message);
                    Marshal.FreeHGlobal(response);
                }

                //uint status = FilterSendMessage(portHandle,)



            }
            else
            {
                throw new Exception("[-] Le portHandle est null, une erreur non détécté à dû se passer");
            }
        }

        public static void Disconnect()
        {
            Console.WriteLine("[!] Tentative de Déconnexion du Driver");
            if (portHandle != null)
            {
                portHandle.Dispose();
            }
            portHandle = null;
            Console.WriteLine("[+] Tentative Réussi !");
        }

    }
   

}

