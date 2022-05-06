# Blog Platform ([Demo](https://projectcode9.com/Blog/jake))
They reset every 20 minutes, so feel free to test them!

<img src="https://projectcode9.com/images/blog1.jpg" width="270"> <img src="https://projectcode9.com/images/blog2.jpg" width="270"> <img src="https://projectcode9.com/images/blog3.jpg" width="270">

A blog platform where anyone can start blogging in their own space.  
Developed with ASP.NET Core 6.0 MVC, Entity Framework 6, PostgreSQL 14, Bootstrap 5 and FontAwesome 5. (Test ID: test@local.com PW: Test2#)

## Security
- HTTPS Connection
- Safe from XSS
- Safe from SQL Injection
- Authorization with custom roles

## Features

On this website, users:

- can create their own blog space.
- are given a unique address for their blog.
- can manage categories.
- can choose category view type.
- can choose the number of articles to show on a page.
- can post/edit/delete their own postings.
- can designate multiple tags to each article.
- can designate a unique URL to each article.

On this website, any person:

- can visit users' postings with a URL such as https://projectcode9.com/Blog/jake/today-is-a-great-day.
- can sort articles by tags.
https://projectcode9.com/Blog/jake/tag/AZURE will show Jake's postings that contain the AZURE tag.
- can sort articles by categories.
https://projectcode9.com/Blog/keanu/category/movie will show Keanu's postings in his Movie category.
- can leave comments on any article anonymously with a password so that they can edit or delete the comment. These passwords are saved safely after being hashed using HMACSHA256 encryption.

---

It was my first project that uses the ASP.NET Core.
I learned great amount of base knowledge regarding how ASP.NET Core MVC, Bootstrap, authentication, authorization, Entity Framework and C# LINQ work together.
