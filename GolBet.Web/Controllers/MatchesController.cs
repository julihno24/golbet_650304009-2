// GolBet.Web/Controllers/MatchesController.cs 
using Microsoft.AspNetCore.Authorization;
using GolBet.Repositories.Data;
using GolBet.Entities.Enums;
using GolBet.Services.DTOs;
using GolBet.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GolBet.Web.Controllers;

public class MatchesController : Controller
{
    private readonly IMatchService _matchService;
    private readonly ITeamService _teamService;

    public MatchesController(IMatchService matchService, ITeamService teamService)
    {
        _matchService = matchService;
        _teamService = teamService;
    }

    public async Task<IActionResult> Index(MatchStatus? status, string? search = null)
    {
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search;
        var board = await _matchService.GetBoardAsync(status, search);
        return View(board);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var match = await _matchService.GetDetailAsync(id);
        if (match is null) return NotFound();

        return View(match);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Teams = new SelectList(await _teamService.GetAllAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(MatchFormDto model)
    {
        if (ModelState.IsValid)
        {
            await _matchService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Teams = new SelectList(await _teamService.GetAllAsync(), "Id", "Name", model.HomeTeamId);
        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var match = await _matchService.GetForEditAsync(id);
        if (match == null) return NotFound();

        ViewBag.Teams = new SelectList(await _teamService.GetAllAsync(), "Id", "Name", match.HomeTeamId);
        return View(match);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, MatchFormDto model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            await _matchService.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Teams = new SelectList(await _teamService.GetAllAsync(), "Id", "Name", model.HomeTeamId);
        return View(model);
    }

    // --- Acciones del Módulo 9: Iniciar y Cerrar (GET) ---

    // POST /Matches/Start/5
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Start(int id)
    {
        try
        {
            await _matchService.StartMatchAsync(id);
            TempData["Success"] = "¡Partido en juego! Las apuestas quedaron cerradas.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    // GET /Matches/Close/5 -> the score form
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Close(int id)
    {
        var match = await _matchService.GetDetailAsync(id);
        if (match is null) return NotFound();

        ViewBag.Match = match; // header data for the view
        return View(new MatchResultFormDto { MatchId = id });
    }
}