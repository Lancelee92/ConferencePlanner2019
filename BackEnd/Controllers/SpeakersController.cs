using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers {
    [Route ("api/[controller]")]
    [ApiController]
    public class SpeakersController : ControllerBase {
        private readonly ApplicationDbContext _db;

        public SpeakersController (ApplicationDbContext db) {
            _db = db;
        }

        // GET: api/Speakers
        [HttpGet]
        public async Task<ActionResult<List<ConferenceDTO.SpeakerResponse>>> GetSpeakers () {
            //return await _db.Speakers.ToListAsync();
            var speakers = await _db.Speakers.AsNoTracking ()
                .Include (s => s.SessionSpeakers)
                .ThenInclude (ss => ss.Session)
                .Select (s => s.MapSpeakerResponse ())
                .ToListAsync ();

            return speakers;
        }

        // GET: api/Speakers/5
        [HttpGet ("{id}")]
        [HttpGet ("{id:int}")]
        public async Task<ActionResult<ConferenceDTO.SpeakerResponse>> GetSpeaker (int id) {
            var speaker = await _db.Speakers.AsNoTracking ()
                .Include (s => s.SessionSpeakers)
                .ThenInclude (ss => ss.Session)
                .SingleOrDefaultAsync (s => s.ID == id);
            if (speaker == null) {
                return NotFound ();
            }
            var result = speaker.MapSpeakerResponse ();
            return result;
        }

        [HttpPost]
        public async Task<ActionResult<ConferenceDTO.SpeakerResponse>> PostSpeaker (ConferenceDTO.Speaker input) {
            var speaker = new Speaker {
                Name = input.Name,
                WebSite = input.WebSite,
                Bio = input.Bio
            };

            _db.Speakers.Add (speaker);
            await _db.SaveChangesAsync ();

            var result = speaker.MapSpeakerResponse ();

            return CreatedAtAction (nameof (GetSpeaker), new { id = speaker.ID }, result);
        }

        [HttpPut ("{id:int}")]
        public async Task<IActionResult> PutSpeaker (int id, ConferenceDTO.Speaker input) {
            var speaker = await _db.FindAsync<Speaker> (id);

            if (speaker == null) {
                return NotFound ();
            }

            speaker.Name = input.Name;
            speaker.WebSite = input.WebSite;
            speaker.Bio = input.Bio;

            // TODO: Handle exceptions, e.g. concurrency
            await _db.SaveChangesAsync ();

            return NoContent ();
        }

        [HttpDelete ("{id:int}")]
        public async Task<ActionResult<ConferenceDTO.SpeakerResponse>> DeleteSpeaker (int id) {
            var speaker = await _db.FindAsync<Speaker> (id);

            if (speaker == null) {
                return NotFound ();
            }

            _db.Remove (speaker);
            await _db.SaveChangesAsync ();

            return speaker.MapSpeakerResponse ();
        }

        private bool SpeakerExists (int id) {
            return _db.Speakers.Any (e => e.ID == id);
        }
    }
}