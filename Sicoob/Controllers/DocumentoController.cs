using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Sicoob.Data;
using Sicoob.Models;
using Sicoob.Models.ViewModels;
using Sicoob.Service;



namespace Sicoob.Controllers
{


    [Authorize]
    public class DocumentoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly LogService _logService;
        
        public DocumentoController(AppDbContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }
        
        private async Task<string> GerarNumeroDocumento(string tipo)
        {
            var anoAtual = DateTime.Now.Year;
            var mesAtual = DateTime.Now.Month;
            
            var quantidadenoAno = await _context.Documentos
                .CountAsync(d => d.TipoDocumento == tipo && d.DataCadastro.Year == anoAtual);

            var proximoSequencial = quantidadenoAno + 1;

            var numeroGerado = $"{tipo}-{proximoSequencial:D4}/{mesAtual:D2}-{anoAtual}";

            return numeroGerado;
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DocumentosViewModel models)
        {
            if (!ModelState.IsValid)
            {
                return View(models);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            const int maxTentativas = 3;

            for (int tentativa = 1; tentativa <= maxTentativas; tentativa++)
            {
                using var transacao = await _context.Database.BeginTransactionAsync();

                try
                {
                    var numeroGerado = await GerarNumeroDocumento(models.TipoDocumento);

                    var documento = new Documento
                    {
                        TipoDocumento = models.TipoDocumento,
                        NumeroDocumento = numeroGerado,
                        DataDocumento = models.DataDocumento,
                        Assunto = models.Assunto,
                        Destinatario = models.Destinatario,
                        Setor = models.Setor,
                        Descricao = models.Descricao,
                        UsuarioId = userId,
                        DataCadastro = DateTime.Now,
                        Status = "Ativo"
                    };

                    _context.Documentos.Add(documento);
                    await _context.SaveChangesAsync();
                    await transacao.CommitAsync();
                    await _logService.RegistrarAsync("Criação de Documento", $"Documento {numeroGerado} criado.");

                    TempData["Mensagem"] = $"Documento {numeroGerado} cadastrado com sucesso!";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException)
                {
                    await transacao.RollbackAsync();
                }
            }

            ModelState.AddModelError("", "Não foi possível gerar o número do documento. Tente novamente.");
            return View(models);
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var documento = await _context.Documentos.FindAsync(id);

            if (documento == null)
            {
                return NotFound();
            }

            if (!UsuarioPodeEditar(documento))
            {
                return Forbid();
            }

            var model = new DocumentosViewModel
            {
                TipoDocumento = documento.TipoDocumento,
                DataDocumento = documento.DataDocumento,
                Assunto = documento.Assunto,
                Destinatario = documento.Destinatario,
                Setor = documento.Setor,
                Descricao = documento.Descricao
            };

            ViewBag.DocumentoId = documento.Id;
            ViewBag.NumeroDocumento = documento.NumeroDocumento;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, DocumentosViewModel model)
        {
            var documento = await _context.Documentos.FindAsync(id);

            if (documento == null)
            {
                return NotFound();
            }

            if (!UsuarioPodeEditar(documento))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DocumentoId = documento.Id;
                ViewBag.NumeroDocumento = documento.NumeroDocumento;
                return View(model);
            }

            documento.DataDocumento = model.DataDocumento;
            documento.Assunto = model.Assunto;
            documento.Destinatario = model.Destinatario;
            documento.Setor = model.Setor;
            documento.Descricao = model.Descricao;

            await _context.SaveChangesAsync();
            await _logService.RegistrarAsync("Alteração de Documento", $"Documento {documento.NumeroDocumento} alterado.");

            TempData["Mensagem"] = "Documento atualizado com sucesso!";
            return RedirectToAction("Index");
        }

        private bool UsuarioPodeEditar(Documento documento)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var perfil = User.FindFirst(ClaimTypes.Role)?.Value;

            return perfil == "Admin" || documento.UsuarioId == userId;
        }
        
        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            var documento = await _context.Documentos.FindAsync(id);

            if (documento == null)
            {
                return NotFound();
            }

            if (!UsuarioPodeEditar(documento))
            {
                return Forbid();
            }

            documento.Status = "Cancelado";
            await _context.SaveChangesAsync();
            await _logService.RegistrarAsync("Exclusão de Documento", $"Documento {documento.NumeroDocumento} cancelado.");

            TempData["Mensagem"] = $"Documento {documento.NumeroDocumento} cancelado.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index(string? numero, string? tipo, string? assunto, DateTime? data,
            string? responsavel, string? destinatario, int pagina = 1)
        {
            const int itensPorPagina = 10;

            var query = _context.Documentos.Include(d => d.Usuario).AsQueryable();

            if (!string.IsNullOrWhiteSpace(numero))
            {
                query = query.Where(d => d.NumeroDocumento.Contains(numero));
            }

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                query = query.Where(d => d.TipoDocumento == tipo);
            }

            if (!string.IsNullOrWhiteSpace(assunto))
            {
                query = query.Where(d => d.Assunto.Contains(assunto));
            }

            if (data.HasValue)
            {
                query = query.Where(d => d.DataDocumento.Date == data.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(responsavel))
            {
                query = query.Where(d => d.Usuario.Nome.Contains(responsavel));
            }

            if (!string.IsNullOrWhiteSpace(destinatario))
            {
                query = query.Where(d => d.Destinatario.Contains(destinatario));
            }
            
            var totalDocumentos = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalDocumentos / (double)itensPorPagina);

            var documentos = await query
                .OrderByDescending(d => d.DataCadastro)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();


            ViewBag.FiltroNumero = numero;
            ViewBag.FiltroTipo = tipo;
            ViewBag.FiltroAssunto = assunto;
            ViewBag.FiltroData = data?.ToString("yyyy-MM-dd");
            ViewBag.FiltroResponsavel = responsavel;
            ViewBag.FiltroDestinatario = destinatario;
            
            ViewBag.PaginaAtual = pagina; 
            ViewBag.TotalPaginas = totalPaginas;
            
            return View(documentos);
                
        }


        

        public async Task<IActionResult> Detalhes(int id)
        {
            var documento = await _context.Documentos
                .Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (documento == null)
            {
                return NotFound();
            }

            var anexos = await _context.Anexos
                .Include(a => a.Usuario)
                .Where(a => a.DocumentoId == documento.Id)
                .OrderByDescending(a => a.Versao)
                .ToListAsync();
            
            ViewBag.Anexos = anexos;
            return View(documento);


        }



    }
}