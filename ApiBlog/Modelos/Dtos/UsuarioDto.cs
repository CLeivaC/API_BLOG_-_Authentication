using System.ComponentModel.DataAnnotations;

namespace ApiBlog.Modelos.Dtos
{
    public class UsuarioDto
    {
      
        public int Id { get; set; }
        public string NombreUsuario { get; set; }
        public string Nombre { get; set; }      
        public string Email { get; set; }
    }
}
