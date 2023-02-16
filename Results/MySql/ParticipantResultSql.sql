select		ec.shortName								Klass,
			concat_ws(' ', p.firstName, p.familyName)	Namn,
			o.shortName									Klubb,
			case when r.runnerStatus = 'passed' then sec_to_time(r.totalTime/100) else null end 
														Tid,
			r.runnerStatus								"Status"
from		eventclasses	ec
left join	entries			e	on	ec.eventClassId = e.acceptedEventClassId
left join	results			r	on	e.entryId = r.entryId
left join	persons			p	on	p.personId = e.competitorId
left join	organisations	o	on	o.organisationId = p.defaultOrganisationId
where		e.eventId = 1
order by	5, ec.sequence, 
			case	when r.runnerStatus = 'passed' 		then r.totalTime 
					when r.runnerStatus = 'notStarted'	then 10000002
														else 10000001
			end