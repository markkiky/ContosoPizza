using ContosoPizza.Models;
using ContosoPizza.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContosoPizza.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class PizzaController : Controller
    {
        public PizzaController() { }

        [HttpGet]
        public ActionResult<List<Pizza>> GetAll() => PizzaService.GetAll();

        [HttpGet("{id}")]
        public ActionResult<Pizza> Get(int id)
        {
            var pizza = PizzaService.Get(id);
            if (pizza == null)
                return NotFound();
            
           return  pizza;
        }
        [HttpPost]
        public ActionResult<Pizza> Post(Pizza pizza)
        {
             PizzaService.Add(pizza);

            return pizza;
        }

        [HttpPut("{id}")]
        public ActionResult<Pizza> Put(int id, Pizza pizza)
        {
            PizzaService.Update(pizza);
            return pizza;
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            PizzaService.Delete(id);
            return Ok();
        }
    }
}
