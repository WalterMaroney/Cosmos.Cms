# Cosmos HL Origin Story

The 2018 California fire season was the worst in that state's history. Almost 2 million acres burned, and tens of thousands of
damanged or lost structures, and over 25 billion of of dollars in property damange. Strong winds and downed power lines contributed to
the confligration. By the end 97 citizens and 6 fire fighters lost thier lives. 85 of the fatalities were from the Camp Fire--which
burned the Butte County town of Paridise to the ground.

## Fall 2019 - Necessity

October 2019 the winds and high fire danger were back. To prevent downed powerlines from igniting more fires, utilities shut off power to
some three million customers. In mid October, the Kincade Fire ignited near Geyserville in Sonoma County. It would eventually force large
scale evacuations of Healdsburg, Windsor and parts of Santa Rosa as strong winds drove the fire south and west. Getting near-realtime information
about power shut off areas, fires, evacuations, and shelter locations out to the public became an imperative.

In response the Governors office with the Government Operations agency tasked the Department of Technology (CDT) to setup a
website that consolidated important public information on a single website that performed well on low-bandwidth cell connections.
Moreover the site was built to sustain sizable bursts of demand in the hundreds of thousands of users per hour.

Drawing on previous experience in similar circumstances, overnight CDT built a high performance website that took advange for cloud-based
services that included Content Delivery Networks and high availability PaaS services.

In this first iteration--the website itself was a simple HTML, CSS and JavaScript based site--also known as a "static" website.
Innovation manly involved in the implementation of cloud infrastructure.

Because this was a static website, CDT had to schedule shifts of developers to be on call 24x7 to make changes to the website regardless
of what time of day or night it was.

# 2020 Something Lightweight and Fast

The new year began with a new charge. In 2019 the website required around the clock teams of developers to make changes to the website.
Long term this is not sustainable, so the task was to create a website where public relations staff could update the site on thier own
without the need of developers.

This requires a dynamic website, able to update with user input. Problem is that dynamic sites tend to not perform as well as static websites. The objective handed down in early 2020 is to create a dynamic website that performs as the original.
