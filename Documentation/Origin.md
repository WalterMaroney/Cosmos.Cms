# Cosmos HL Origin Story

The 2018 California fire season was the worst in that state's history.vWinds and downed power lines cause fire to spread across 2 million acres, and tens of thousands of
structures damaged or lost, and and over 25 billion of dollars worth in property damange.  Nity seven citizens and six fire fighters lost thier lives that year. And 85 fatalities were due to a single fire, called the the "Camp Fire."  The Butte County town of Paridise burnt to the ground.

## Fall 2019 - Necessity

Then in October 2019 it happend again.  Utilities tried to prevent harm by cutting power to
some three million customers. It wasn't enough. High winds and downed powerlines ignited fires again.  

Mid October the Kincade Fire ignited near Geyserville in Sonoma County. It forced mass evacuations of Healdsburg, Windsor and parts of Santa Rosa. Strong winds drove the fire south and west. Tens of thousands of people were on the move and getting near-realtime information about power shut off areas, fires, evacuations, and shelter locations out to the public became an imperative.  California needed a website that would be extremely reliable and perform extremely well under low bandwidth connections.

In response the Governors Office and Government Operations agency tasked the Department of Technology (CDT) to setup a
website that consolidated important public information on a single website that performed well on low-bandwidth cell connections--and was highly available as it must not fail.

Moreover the site was built to sustain sizable bursts of demand in the hundreds of thousands of users per hour.

Drawing on previous experience in similar circumstances, overnight CDT built a high performance website that took advange for cloud-based
services that included Content Delivery Networks and high availability PaaS services.

In this first iteration--the website itself was a simple HTML, CSS and JavaScript based site--also known as a "static" website.
Innovation manly involved in the implementation of cloud infrastructure.

Because this was a static website, CDT had to schedule shifts of developers to be on call 24x7 to make changes to the website regardless
of what time of day or night it was.

## 2020 - COVID 19

The website built in 2019 was fast, efficient and reliable.  In 2019 the website required around the clock teams of developers to make changes to the website. This required a high degree of labor.  Public information officers would send information to developers who would then write HTML, CSS and JavaScript to make the content appear on the website.

Long term this is not sustainable, so the task was to create a website where public relations staff could update the site on thier own
without the need of developers. 

This requires a dynamic website, able to update with user input. Problem is that dynamic sites have vulnerabilities. They break, don't perform well, and place strains on infrastructure.

What was needed was a content management system that achieved the following:

* Performed as well as a state website in terms of speed and load.
* Simplisitc in nature, allowing a non-techical person with little or no training update content.
* Very light weight, required little or no extra configuration to serve hundreds of thousands or millions of users.
* As reliable as a state website, perhaps even more resiliant.


