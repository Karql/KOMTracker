# https://stackoverflow.com/questions/59189311/edit-hide-nginx-server-header-under-alpine-linux
ARG VERSION=1.31-alpine
FROM nginx:${VERSION} as builder

ENV MORE_HEADERS_VERSION=0.37
ENV MORE_HEADERS_GITREPO=openresty/headers-more-nginx-module
ENV NGX_BROTLI_GITREPO=google/ngx_brotli

# Download sources
RUN wget "http://nginx.org/download/nginx-${NGINX_VERSION}.tar.gz" -O nginx.tar.gz && \
    wget "https://github.com/${MORE_HEADERS_GITREPO}/archive/v${MORE_HEADERS_VERSION}.tar.gz" -O extra_module.tar.gz

# For latest build deps, see https://github.com/nginx/docker-nginx/blob/1.31.2/mainline/alpine/Dockerfile
RUN  apk add --no-cache --virtual .build-deps \
    gcc \
    libc-dev \
    make \
    openssl-dev \
    pcre2-dev \
    zlib-dev \
    linux-headers \
    libxslt-dev \
    gd-dev \
    geoip-dev \
    libedit-dev \
    bash \
    alpine-sdk \
    findutils \
    curl \
    cargo \
    clang-libclang \
    cmake

SHELL ["/bin/ash", "-eo", "pipefail", "-c"]

RUN rm -rf /usr/src/nginx /usr/src/extra_module && mkdir -p /usr/src/nginx /usr/src/extra_module && \
    tar -zxC /usr/src/nginx -f nginx.tar.gz && \
    tar -xzC /usr/src/extra_module -f extra_module.tar.gz

# ngx_brotli requires its bundled brotli submodule (it does NOT link the system libbrotli).
# Clone with submodules, then build the bundled brotli as static libs with PIC (they get linked
# into the dynamic .so module). git comes from alpine-sdk.
RUN git clone --depth=1 --recurse-submodules --shallow-submodules \
        "https://github.com/${NGX_BROTLI_GITREPO}.git" /usr/src/ngx_brotli && \
    mkdir -p /usr/src/ngx_brotli/deps/brotli/out && \
    cd /usr/src/ngx_brotli/deps/brotli/out && \
    cmake -DCMAKE_BUILD_TYPE=Release -DBUILD_SHARED_LIBS=OFF -DCMAKE_POSITION_INDEPENDENT_CODE=ON .. && \
    cmake --build . --config Release --target brotlienc brotlidec brotlicommon

WORKDIR /usr/src/nginx/nginx-${NGINX_VERSION}

# Reuse same cli arguments as the nginx:alpine image used to build
RUN CONFARGS=$(nginx -V 2>&1 | sed -n -e 's/^.*arguments: //p') && \
    sh -c "./configure --with-compat $CONFARGS --add-dynamic-module=/usr/src/extra_module/* --add-dynamic-module=/usr/src/ngx_brotli" && make modules


# Production container starts here
FROM nginx:${VERSION}

# ngx_brotli links the bundled brotli statically (BUILD_SHARED_LIBS=OFF), so no runtime lib needed.
COPY --from=builder /usr/src/nginx/nginx-${NGINX_VERSION}/objs/*_module.so /etc/nginx/modules/
