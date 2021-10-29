# Cosmos HL Origin Story

The 2018 fire season was the worst in California history.  Wind-driven fires ignited by downed power lines cause fire to spread across 2 million acres, and tens of thousands of
structures damaged or lost, and and over 25 billion of dollars worth in property damange.  Ninety seven citizens and six fire fighters lost thier lives that year. And 85 fatalities were due to a single fire, called the the "Camp Fire."  The Butte County town of Paridise burnt to the ground.

## Fall 2019

In mid October 2019 a new fire stated near the town Geyserville in Sonoma County.  It became known as the Kincade Fire.  Strong north winds drives it south forcing mass evacuations of Healdsburg, Windsor and parts of Santa Rosa. Tens of thousands of people were suddenly on the move, and tens of thousands more worried that they might be next.  Friends and family from throughout California and beyond wanted to know what was happening.

A majority of people were trying to access State of California websites for information, and most were using cell phone.  For many they were doing so over cell phone networks in rural areas or n conjested urban areas.  Everyone with a cell phone was competing for limited network bandwidth.  People were looking to state websites for information.

Early on three problems were recognized. First, the majority of websites were designed for high-badwidth connections--which was not the case now.  Second, most of the websites were not built to sustain extremely high user demands, and third people had to visit multiple websites to get information.

California needed a single website that consolidated information and is designed for low-bandwidth connections and able to sustain high user loads.

## Simple Design

In response the Governors Office and Government Operations agency tasked the Department of Technology (CDT) to setup a new website that would consolidate information about the fires and work well over limited bandwidth and on cell phones.  The deadline was to do this overnight.  Moreover this website _could not got down_ once launched.

At first CDT staff considered using WordPress or another content management system.  But these websites historically were not high performers. Their complexity also meant there was more that can go wrong, and setting these systems up to sustain a high load would take time and testing.  Moreover, CDT wanted the website to be hosted simultaeously in multiple regions--and keeping these websites in sync dynamically would not likely happen overnight.

The decisions was to build something simple so it would be fast and less vulnerable to errors and more likely to stay up and running.

CDT created a "static" website hosted simultaeously in multiple geographic regions in the cloud.  Web pages were designed without the use of unecessary graphics or other assets, making the number of bytes sent over the wire for each page was kept to a minimum.  In addition, CDT installed a Content Distribution Network or CDN to support high user loads.

This static website design offered several advantages.  It was highly reliable and very fast at serving web pages.

## Labor Intensive

The website built in 2019 was fast, efficient and reliable.  Public information officers would send information to developers who would then update web pages on the website.  Changes were pushed to the website instances using a DevOps pipleline.  To make this work teams of developers and DevOps engineers needed to work around the clock.  In the short term this worked, but long term this is not sustainable. The realization came that some kind of a CMS would be needed in the future.  Something as reliable and as fast as a state website was needed that didn't need around the clock staffing of technical staff.

What was needed was something that was a cross between a static website and a dynamic CMS.
 
What was needed was a _dynamic_ content management system that achieved the following:

* Performed as well as a state website in terms of speed and load.
* Simplisitc in nature, allowing a non-techical person with little or no training update content.
* Very light weight, required little or no extra configuration to serve hundreds of thousands or millions of users.
* As reliable as a state website, perhaps even more resiliant.

