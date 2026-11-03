USE sakila;
GO

-- Enunciado 1: 10 películas con mayor cantidad de alquileres.
SELECT TOP 10 f.title AS pelicula, COUNT(r.rental_id) AS alquileres
FROM film f
INNER JOIN inventory i ON i.film_id = f.film_id
INNER JOIN rental r    ON r.inventory_id = i.inventory_id
GROUP BY f.film_id, f.title
ORDER BY alquileres DESC, f.title;
GO

-- Enunciado 3: 10 películas con al menos una copia disponible para alquilar.
SELECT TOP 10 f.title AS pelicula, COUNT(*) AS copias_disponibles
FROM film f
INNER JOIN inventory i ON i.film_id = f.film_id
WHERE NOT EXISTS (SELECT 1 FROM rental r
                  WHERE r.inventory_id = i.inventory_id AND r.return_date IS NULL)
GROUP BY f.film_id, f.title
ORDER BY copias_disponibles DESC, f.title;
GO

-- Enunciado 7: 5 clientes con más alquileres sin devolver.
SELECT TOP 5 c.first_name + ' ' + c.last_name AS cliente, COUNT(*) AS pendientes
FROM customer c
INNER JOIN rental r ON r.customer_id = c.customer_id
WHERE r.return_date IS NULL
GROUP BY c.customer_id, c.first_name, c.last_name
ORDER BY pendientes DESC, cliente;
GO
