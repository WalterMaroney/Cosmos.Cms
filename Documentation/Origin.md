# Cosmos HL Origin Story

The 2018 California fire season was the worst in that state's history.vWinds and downed power lines cause fire to spread across 2 million acres, and tens of thousands of
structures damaged or lost, and and over 25 billion of dollars worth in property damange.  Nity seven citizens and six fire fighters lost thier lives that year. And 85 fatalities were due to a single fire, called the the "Camp Fire."  The Butte County town of Paridise burnt to the ground. Thousands of people needed information. Most on cell phones over low-bandwidth connections.  Up until then state websites assumed people would access websites at home--and with high bandwidth connections.

With thousands of people with cell phones competing with limited bandwidth over streatched cell phone towers, existing state websites where not built with that in mind.

## Fall 2019 - Necessity

Then in October 2019 the winds and high fire danger were back. This time to prevent downed powerlines from igniting more fires utilities cut power to
some three million customers. But things got worse.

Mid October the Kincade Fire ignited near Geyserville in Sonoma County. It forced mass evacuations of Healdsburg, Windsor and parts of Santa Rosa as strong winds drove the fire south and west. Tens of thousands of people were on the move and getting near-realtime information about power shut off areas, fires, evacuations, and shelter locations out to the public became an imperative.

In response the Governors office along with the Government Operations agency tasked the Department of Technology (CDT) to setup a
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
