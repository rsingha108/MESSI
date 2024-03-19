FROM golang

EXPOSE 179

ENV GOPATH  ${HOME}/go
ENV GOROOT /usr/local/go
ENV PATH $GOPATH/bin:$GOROOT/bin:$PATH

RUN git clone https://github.com/osrg/gobgp.git

RUN cd gobgp && go install ./cmd/gobgpd ./cmd/gobgp

# This directory must be mounted as a local volume with '-v `pwd`/gobgp:/etc/gobgp:rw' docker's command line option.
RUN mkdir /etc/gpbgp
