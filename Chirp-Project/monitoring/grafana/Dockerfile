FROM grafana/grafana:12.1

ENV PROMETHEUS_HOST='host.docker.internal:9090'

COPY monitoring/grafana/dashboards /etc/grafana/provisioning/dashboards
COPY monitoring/grafana/datasources /etc/grafana/provisioning/datasources

EXPOSE 3000