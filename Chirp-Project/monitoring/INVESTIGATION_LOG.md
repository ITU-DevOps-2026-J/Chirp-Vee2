# Investigation Log - ITU MiniTwit

Date: 14 April 2026

## Goal

Use Grafana + Prometheus + Loki to answer:

- Slowest endpoint and latency
- Where time is spent in that endpoint
- Who tries to log in
- CPU load (last hour/day)
- Front page average response time
- Amount of registered users
- Average followers per user

## What Was Added

1. Application metrics:

- chirp_http_request_duration_seconds (method, path, status_code)
- chirp_front_page_duration_seconds (status_code)
- chirp_db_command_duration_seconds (path, command_kind)
- chirp_registered_users
- chirp_average_followers_per_user

2. Logging:

- Login attempt logs with identifier and remote IP
- Login outcome logs (succeeded, invalid_password, locked_out, requires_two_factor, user_not_found)

3. Grafana dashboards:

- Minitwit Monitoring Overview (Prometheus)
- Minitwit Logs (Loki)

## How To Answer Each Question

1. Which API endpoint is the slowest? How slow is it?

- Dashboard panel: Slowest API/Page Endpoints
- PromQL:
  topk(1,
  sum by (method, path) (rate(chirp_http_request_duration_seconds_sum[5m]))
  /
  sum by (method, path) (rate(chirp_http_request_duration_seconds_count[5m]))
  )

2. Where is time being spent in this endpoint?

- Dashboard panel: Where Time Is Spent (DB Share by Path)
- PromQL:
  100 \* sum by (path) (rate(chirp_db_command_duration_seconds_sum[5m]))
  /
  clamp_min(sum by (path) (rate(chirp_http_request_duration_seconds_sum[5m])), 0.000001)
- Interpretation:
  High percentage => endpoint is mostly DB-bound.
  Low percentage => time is mostly non-DB work (rendering, middleware, auth, etc).

3. Who tries to log into your server(s)?

- Dashboard panel: Who Tries to Log In (Top 10, Last Hour)
- LogQL:
  topk(10, sum by (login_identifier) (
  count_over_time(
  {service_name="app"} |= "Login attempt for"
  | regexp "Login attempt for (?P<login_identifier>[^ ]+)" [1h]
  )
  ))
- Use the Login Outcomes panel for success/failure context.

4. CPU load during the last hour/the last day

- Panels:
  - CPU Avg Last Hour
  - CPU Avg Last Day
- PromQL:
  avg*over_time((rate(process_cpu_seconds_total{job="itu-minittwit-app"}[5m]) * 100)[1h:5m])
  avg*over_time((rate(process_cpu_seconds_total{job="itu-minittwit-app"}[5m]) * 100)[1d:5m])

5. Average response time of application's front page

- Panel: Front Page Average Response Time
- PromQL:
  rate(chirp_front_page_duration_seconds_sum[5m])
  /
  clamp_min(rate(chirp_front_page_duration_seconds_count[5m]), 0.000001)

6. Amount of users registered in the system

- Panel: Registered Users
- PromQL:
  chirp_registered_users

7. Average amount of followers a user has

- Panel: Average Followers per User
- PromQL:
  chirp_average_followers_per_user

## Stakeholder Categories

1. Slowest endpoint + where time is spent:

- Interested parties: Operators (SRE/DevOps), backend developers, technical leadership.

2. Who tries to log in:

- Interested parties: Security team, operators, compliance/audit stakeholders.

3. CPU load (hour/day):

- Interested parties: Operators (capacity and reliability), technical leadership.

4. Front page response time:

- Interested parties: Product owners, operators, frontend/backend developers.

5. Registered users:

- Interested parties: Business/product department, leadership.

6. Average followers per user:

- Interested parties: Business/product department, data/analytics.

## Most Important Category

Most important category: Operators/SRE.

Role perspective: Reliability/Operations.

Reason:

- If the system is not stable or observable, business and product metrics become unreliable.
- Operational visibility (latency, CPU, DB-time share, auth behavior) enables quick incident response and safe optimization.
