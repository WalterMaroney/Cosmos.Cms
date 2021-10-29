# Cosmos HL Origin Story

The 2018 fire season was the worst in California history.  Wind-driven fires ignited by downed power lines cause fire to spread across 2 million acres, and tens of thousands of
structures damaged or lost, and and over 25 billion of dollars worth in property damange.  Ninety seven citizens and six fire fighters lost thier lives that year. And 85 fatalities were due to a single fire, called the the "Camp Fire."  The Butte County town of Paridise burnt to the ground.

## Fall 2019

Then in October 2019 it happend again and it was worse.  This time utilities tried to prevent fires by cutting power to three million customers. And yet fires spread again.

In mid October a new fire stated near the town Geyserville in Sonoma County.  It became known as the Kincade Fire.  Strong north winds drives it south forcing mass evacuations of Healdsburg, Windsor and parts of Santa Rosa. Tens of thousands of people were suddenly on the move, and tens of thousands more worried that they might be next.  Friends and family from throughout California and beyond wanted to know what was happening.

A majority of people were trying to access State of California websites for information, and most were using cell phone.  For many they were doing so over cell phone networks in rural areas or conjested urban areas--all competing for limited network bandwidth.  People were looking to state websites for information.

Early on three problems were recognized. First, the majority of websites were designed for high-badwidth connections--which was not the case now.  Second, most of the websites were not built to sustain extremely high user demands, and third people had to visit multiple websites to get information.

California needed a single website that consolidated information and is designed for low-bandwidth connections and able to sustain high user loads.

## Simple Design

In response the Governors Office and Government Operations agency tasked the Department of Technology (CDT) to setup a website capabile of doing this, and the deadline was to do this overnight.  And this website could not got down, so it had to be highly reliable and available.

At first CDT staff considered using WordPress or another content management system.  But these websites historically were not high performers. Their complexity also meant there was more that can go wrong, and setting these systems up to sustain a high load would take time and testing.  Moreover, CDT wanted the website to be hosted simultaeously in multiple regions--and keeping these websites in sync dynamically would not likely happen overnight.

Drawing on previous experience in similar circumstances, overnight CDT built a high performance website that took advange for cloud-based services that included Content Delivery Networks and high availability PaaS services.


CDT assembled a "static" website hosted simultaeously in multiple geographic regions in the cloud.  The web pages were designed without the use of unecessary graphics or other assets, making the number of bytes sent over the wire for each page was kept to a minimum.  In addition, CDT installed a Content Distribution Network or CDN to support high user loads.

This static website design offered several advantages.  It was highly reliable and very fast at serving web pages.

## 2020 - COVID 19

The website built in 2019 was fast, efficient and reliable.  In 2019 the website required around the clock teams of developers to make changes to the website. This required a high degree of labor.  Public information officers would send information to developers who would then write HTML, CSS and JavaScript to make the content appear on the website.

Long term this is not sustainable, so the task was to create a website where public relations staff could update the site on thier own
without the need of developers. 

This requires a dynamic website, able to update with user input. Problem is that dynamic sites have vulnerabilities. They break, don't perform well, and place strains on infrastructure.

What was needed was a _dynamic_ content management system that achieved the following:

* Performed as well as a state website in terms of speed and load.
* Simplisitc in nature, allowing a non-techical person with little or no training update content.
* Very light weight, required little or no extra configuration to serve hundreds of thousands or millions of users.
* As reliable as a state website, perhaps even more resiliant.

