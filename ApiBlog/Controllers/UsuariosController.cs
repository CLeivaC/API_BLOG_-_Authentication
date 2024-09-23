using ApiBlog.Modelos;
using ApiBlog.Modelos.Dtos;
using ApiBlog.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ApiBlog.Controllers
{
    [Route("api/usuarios")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        protected RespuestasAPI _respuestasAPI;
        private readonly IMapper _mapper;

        public UsuariosController(IUsuarioRepositorio usuarioRepo, IMapper mapper)
        {
            _usuarioRepo = usuarioRepo;
            _mapper = mapper;
            this._respuestasAPI = new();
        }

        
        [HttpPost("registro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public  async Task<IActionResult> Registro([FromBody] UsuarioRegistroDto usuarioRegistroDto)
        {
            bool validarNombreUsuarioUnico = _usuarioRepo.IsUniqueUser(usuarioRegistroDto.NombreUsuario);
            if (!validarNombreUsuarioUnico)
            {
                _respuestasAPI.StatusCode = HttpStatusCode.BadRequest;
                _respuestasAPI.IsSuccess = false;
                _respuestasAPI.ErrorMessages.Add("El nombre de usuario ya existe");
                return BadRequest(_respuestasAPI);
            }

            var usuario = await _usuarioRepo.Registro(usuarioRegistroDto);
            if (usuario == null)
            {
                _respuestasAPI.StatusCode = HttpStatusCode.BadRequest;
                _respuestasAPI.IsSuccess = false;
                _respuestasAPI.ErrorMessages.Add("Error en el registro");
                return BadRequest(_respuestasAPI);
            }
            _respuestasAPI.StatusCode = HttpStatusCode.OK;
            _respuestasAPI.IsSuccess = true;

            return Ok(_respuestasAPI);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UsuarioLoginDto usuarioLoginDto)
        {
           
            

            var respuestaLogin = await _usuarioRepo.Login(usuarioLoginDto);
            if (respuestaLogin.Usuario == null || string.IsNullOrEmpty(respuestaLogin.Token))
            {
                _respuestasAPI.StatusCode = HttpStatusCode.BadRequest;
                _respuestasAPI.IsSuccess = false;
                _respuestasAPI.ErrorMessages.Add("El nombre de usuario o password son incorrectos");
                return BadRequest(_respuestasAPI);
            }
            _respuestasAPI.StatusCode = HttpStatusCode.OK;
            _respuestasAPI.IsSuccess = true;
            _respuestasAPI.Result = respuestaLogin;

            return Ok(_respuestasAPI);
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUsuarios()
        {
            var listaUsuarios = _usuarioRepo.GetUsuarios();
            var listaUsuariosDto = new List<UsuarioDto>();

            foreach (var lista in listaUsuarios)
            {
                listaUsuariosDto.Add(_mapper.Map<UsuarioDto>(lista));
            }
            return Ok(listaUsuariosDto);
        }

        [Authorize]
        [HttpGet("{usuarioId:int}",Name ="GetUsuario")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult GetUsuario(int usuarioId)
        {
            var itemUsuario = _usuarioRepo.GetUsuario(usuarioId);

            if(itemUsuario == null)
            {
                return NotFound();
            }
            var itemUsuarioDto = _mapper.Map<UsuarioDto>(itemUsuario);
            return Ok(itemUsuarioDto);
        }

    }
}
