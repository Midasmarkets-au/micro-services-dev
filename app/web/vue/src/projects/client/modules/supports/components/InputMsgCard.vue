<template>
  <div class="card">
    <!--begin::Body-->
    <div class="card-body p-lg-17">
      <!--begin::Row-->
      <div class="row mb-3">
        <!--begin::Col-->
        <div class="col-md-6 pe-lg-10">
          <!--begin::form-->
          <form
            action=""
            class="form mb-15 fv-plugins-bootstrap5 fv-plugins-framework"
            method="post"
            id="kt_contact_form"
          >
            <h1 class="fw-bold text-dark mb-9">Send Us Email</h1>

            <!--begin::Input group-->
            <div class="row mb-5">
              <!--begin::Col-->
              <div class="col-md-6 fv-row fv-plugins-icon-container">
                <!--begin::Label-->
                <label class="fs-5 fw-semibold mb-2">Name</label>
                <!--end::Label-->

                <!--begin::Input-->
                <input
                  type="text"
                  class="form-control form-control-solid"
                  placeholder=""
                  name="name"
                />
                <!--end::Input-->
                <div
                  class="fv-plugins-message-container invalid-feedback"
                ></div>
              </div>
              <!--end::Col-->

              <!--begin::Col-->
              <div class="col-md-6 fv-row fv-plugins-icon-container">
                <!--end::Label-->
                <label class="fs-5 fw-semibold mb-2">Email</label>
                <!--end::Label-->

                <!--end::Input-->
                <input
                  type="text"
                  class="form-control form-control-solid"
                  placeholder=""
                  name="email"
                />
                <!--end::Input-->
                <div
                  class="fv-plugins-message-container invalid-feedback"
                ></div>
              </div>
              <!--end::Col-->
            </div>
            <!--end::Input group-->

            <!--begin::Input group-->
            <div class="d-flex flex-column mb-5 fv-row">
              <!--begin::Label-->
              <label class="fs-5 fw-semibold mb-2">Subject</label>
              <!--end::Label-->

              <!--begin::Input-->
              <input
                class="form-control form-control-solid"
                placeholder=""
                name="subject"
              />
              <!--end::Input-->
            </div>
            <!--end::Input group-->

            <!--begin::Input group-->
            <div
              class="d-flex flex-column mb-10 fv-row fv-plugins-icon-container"
            >
              <label class="fs-6 fw-semibold mb-2">Message</label>

              <textarea
                class="form-control form-control-solid"
                rows="6"
                name="message"
                placeholder=""
              >
              </textarea>
              <div class="fv-plugins-message-container invalid-feedback"></div>
            </div>
            <!--end::Input group-->

            <!--begin::Submit-->
            <button
              type="submit"
              class="btn btn-primary"
              id="kt_contact_submit_button"
            >
              <!--begin::Indicator label-->
              <span class="indicator-label"> Send Feedback</span>
              <!--end::Indicator label-->

              <!--begin::Indicator progress-->
              <span class="indicator-progress">
                Please wait...
                <span
                  class="spinner-border spinner-border-sm align-middle ms-2"
                ></span>
              </span>
              <!--end::Indicator progress-->
            </button>
            <!--end::Submit-->
          </form>
          <!--end::form-->
        </div>
        <!--end::Col-->

        <!--begin::Col-->
        <div class="col-md-6 ps-lg-10">
          <!--begin::Map-->
          <div
            id="kt_contact_map"
            class="w-100 rounded mb-2 mb-lg-0 mt-2 leaflet-container leaflet-touch leaflet-fade-anim leaflet-grab leaflet-touch-drag leaflet-touch-zoom"
            style="height: 486px; position: relative"
            tabindex="0"
          >
            <div
              class="leaflet-pane leaflet-map-pane"
              style="transform: translate3d(0px, 0px, 0px)"
            >
              <div class="leaflet-pane leaflet-tile-pane">
                <div class="leaflet-layer" style="z-index: 1; opacity: 1">
                  <div
                    class="leaflet-tile-container leaflet-zoom-animated"
                    style="
                      z-index: 18;
                      transform: translate3d(0px, 0px, 0px) scale(1);
                    "
                  >
                    <img
                      alt=""
                      src="https://a.tile.openstreetmap.org/18/77197/98549.png"
                      class="leaflet-tile leaflet-tile-loaded"
                      style="
                        width: 256px;
                        height: 256px;
                        transform: translate3d(75px, 143px, 0px);
                        opacity: 1;
                      "
                    /><img
                      alt=""
                      src="https://c.tile.openstreetmap.org/18/77197/98548.png"
                      class="leaflet-tile leaflet-tile-loaded"
                      style="
                        width: 256px;
                        height: 256px;
                        transform: translate3d(75px, -113px, 0px);
                        opacity: 1;
                      "
                    /><img
                      alt=""
                      src="https://c.tile.openstreetmap.org/18/77196/98549.png"
                      class="leaflet-tile leaflet-tile-loaded"
                      style="
                        width: 256px;
                        height: 256px;
                        transform: translate3d(-181px, 143px, 0px);
                        opacity: 1;
                      "
                    /><img
                      alt=""
                      src="https://b.tile.openstreetmap.org/18/77198/98549.png"
                      class="leaflet-tile leaflet-tile-loaded"
                      style="
                        width: 256px;
                        height: 256px;
                        transform: translate3d(331px, 143px, 0px);
                        opacity: 1;
                      "
                    /><img
                      alt=""
                      src="https://b.tile.openstreetmap.org/18/77197/98550.png"
                      class="leaflet-tile leaflet-tile-loaded"
                      style="
                        width: 256px;
                        height: 256px;
                        transform: translate3d(75px, 399px, 0px);
                        opacity: 1;
                      "
                    /><img
                      alt=""
                      src="https://b.tile.openstreetmap.org/18/77196/98548.png"
                      class="leaflet-tile leaflet-tile-loaded"
                      style="
                        width: 256px;
                        height: 256px;
                        transform: translate3d(-181px, -113px, 0px);
                        opacity: 1;
                      "
                    /><img
                      alt=""
                      src="https://a.tile.openstreetmap.org/18/77198/98548.png"
                      class="leaflet-tile leaflet-tile-loaded"
                      style="
                        width: 256px;
                        height: 256px;
                        transform: translate3d(331px, -113px, 0px);
                        opacity: 1;
                      "
                    /><img
                      alt=""
                      src="https://a.tile.openstreetmap.org/18/77196/98550.png"
                      class="leaflet-tile leaflet-tile-loaded"
                      style="
                        width: 256px;
                        height: 256px;
                        transform: translate3d(-181px, 399px, 0px);
                        opacity: 1;
                      "
                    /><img
                      alt=""
                      src="https://c.tile.openstreetmap.org/18/77198/98550.png"
                      class="leaflet-tile leaflet-tile-loaded"
                      style="
                        width: 256px;
                        height: 256px;
                        transform: translate3d(331px, 399px, 0px);
                        opacity: 1;
                      "
                    />
                  </div>
                </div>
              </div>
              <div class="leaflet-pane leaflet-overlay-pane"></div>
              <div class="leaflet-pane leaflet-shadow-pane"></div>
              <div class="leaflet-pane leaflet-marker-pane">
                <div
                  class="leaflet-marker-icon leaflet-marker leaflet-zoom-animated leaflet-interactive"
                  tabindex="0"
                  role="button"
                  style="
                    background-position: -10px -10px;
                    margin-left: -20px;
                    margin-top: -37px;
                    width: 12px;
                    height: 12px;
                    transform: translate3d(311px, 312px, 0px);
                    z-index: 312;
                  "
                >
                  <span class="svg-icon svg-icon-primary shadow svg-icon-3x"
                    ><svg
                      xmlns="http://www.w3.org/2000/svg"
                      xmlns:xlink="http://www.w3.org/1999/xlink"
                      width="24px"
                      height="24px"
                      viewBox="0 0 24 24"
                      version="1.1"
                    >
                      <g
                        stroke="none"
                        stroke-width="1"
                        fill="none"
                        fill-rule="evenodd"
                      >
                        <rect x="0" y="24" width="24" height="0"></rect>
                        <path
                          d="M5,10.5 C5,6 8,3 12.5,3 C17,3 20,6.75 20,10.5 C20,12.8325623 17.8236613,16.03566 13.470984,20.1092932 C12.9154018,20.6292577 12.0585054,20.6508331 11.4774555,20.1594925 C7.15915182,16.5078313 5,13.2880005 5,10.5 Z M12.5,12 C13.8807119,12 15,10.8807119 15,9.5 C15,8.11928813 13.8807119,7 12.5,7 C11.1192881,7 10,8.11928813 10,9.5 C10,10.8807119 11.1192881,12 12.5,12 Z"
                          fill="#000000"
                          fill-rule="nonzero"
                        ></path>
                      </g></svg
                  ></span>
                </div>
              </div>
              <div class="leaflet-pane leaflet-tooltip-pane"></div>
              <div class="leaflet-pane leaflet-popup-pane">
                <div
                  class="leaflet-popup leaflet-zoom-animated"
                  style="
                    opacity: 1;
                    transform: translate3d(311px, 275px, 0px);
                    bottom: -7px;
                    left: -120px;
                  "
                >
                  <div class="leaflet-popup-content-wrapper">
                    <div class="leaflet-popup-content" style="width: 193px">
                      430 E 6th St, New York, 10009.
                    </div>
                  </div>
                  <div class="leaflet-popup-tip-container">
                    <div class="leaflet-popup-tip"></div>
                  </div>
                </div>
              </div>
              <div
                class="leaflet-proxy leaflet-zoom-animated"
                style="
                  transform: translate3d(1.97626e7px, 2.52286e7px, 0px)
                    scale(131072);
                "
              ></div>
            </div>
            <div class="leaflet-control-container">
              <div class="leaflet-top leaflet-left">
                <div class="leaflet-control-zoom leaflet-bar leaflet-control">
                  <a
                    class="leaflet-control-zoom-in leaflet-disabled"
                    href="#"
                    title="Zoom in"
                    role="button"
                    aria-label="Zoom in"
                    aria-disabled="true"
                    ><span aria-hidden="true">+</span></a
                  ><a
                    class="leaflet-control-zoom-out"
                    href="#"
                    title="Zoom out"
                    role="button"
                    aria-label="Zoom out"
                    aria-disabled="false"
                    ><span aria-hidden="true">−</span></a
                  >
                </div>
              </div>
              <div class="leaflet-top leaflet-right"></div>
              <div class="leaflet-bottom leaflet-left"></div>
              <div class="leaflet-bottom leaflet-right">
                <div class="leaflet-control-attribution leaflet-control">
                  <a
                    href="https://leafletjs.com"
                    title="A JavaScript library for interactive maps"
                    ><svg
                      aria-hidden="true"
                      xmlns="http://www.w3.org/2000/svg"
                      width="12"
                      height="8"
                      viewBox="0 0 12 8"
                      class="leaflet-attribution-flag"
                    >
                      <path fill="#4C7BE1" d="M0 0h12v4H0z"></path>
                      <path fill="#FFD500" d="M0 4h12v3H0z"></path>
                      <path fill="#E0BC00" d="M0 7h12v1H0z"></path>
                    </svg>
                    Leaflet</a
                  >
                  <span aria-hidden="true">|</span> ©
                  <a href="https://osm.org/copyright">OpenStreetMap</a>
                  contributors
                </div>
              </div>
            </div>
          </div>
          <!--end::Map-->
        </div>
        <!--end::Col-->
      </div>
      <!--end::Row-->

      <!--begin::Row-->
      <div class="row g-5 mb-5 mb-lg-15">
        <!--begin::Col-->
        <div class="col-sm-6 pe-lg-10">
          <!--begin::Phone-->
          <div
            class="text-center bg-light card-rounded d-flex flex-column justify-content-center p-10 h-100"
          >
            <!--begin::Icon-->
            <!--SVG file not found: icons/duotune/finance/fin006.svgPhone.svg-->
            <!--end::Icon-->

            <!--begin::Subtitle-->
            <h1 class="text-dark fw-bold my-5">Let’s Speak</h1>
            <!--end::Subtitle-->

            <!--begin::Number-->
            <div class="text-gray-700 fw-semibold fs-2">1 (833) 597-7538</div>
            <!--end::Number-->
          </div>
          <!--end::Phone-->
        </div>
        <!--end::Col-->

        <!--begin::Col-->
        <div class="col-sm-6 ps-lg-10">
          <!--begin::Address-->
          <div
            class="text-center bg-light card-rounded d-flex flex-column justify-content-center p-10 h-100"
          >
            <!--begin::Icon-->
            <!--begin::Svg Icon | path: icons/duotune/general/gen018.svg-->
            <span class="svg-icon svg-icon-3tx svg-icon-primary"
              ><svg
                width="24"
                height="24"
                viewBox="0 0 24 24"
                fill="none"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  opacity="0.3"
                  d="M18.0624 15.3453L13.1624 20.7453C12.5624 21.4453 11.5624 21.4453 10.9624 20.7453L6.06242 15.3453C4.56242 13.6453 3.76242 11.4453 4.06242 8.94534C4.56242 5.34534 7.46242 2.44534 11.0624 2.04534C15.8624 1.54534 19.9624 5.24534 19.9624 9.94534C20.0624 12.0453 19.2624 13.9453 18.0624 15.3453Z"
                  fill="currentColor"
                ></path>
                <path
                  d="M12.0624 13.0453C13.7193 13.0453 15.0624 11.7022 15.0624 10.0453C15.0624 8.38849 13.7193 7.04535 12.0624 7.04535C10.4056 7.04535 9.06241 8.38849 9.06241 10.0453C9.06241 11.7022 10.4056 13.0453 12.0624 13.0453Z"
                  fill="currentColor"
                ></path>
              </svg>
            </span>
            <!--end::Svg Icon-->
            <!--end::Icon-->

            <!--begin::Subtitle-->
            <h1 class="text-dark fw-bold my-5">Our Head Office</h1>
            <!--end::Subtitle-->

            <!--begin::Description-->
            <div class="text-gray-700 fs-3 fw-semibold">
              Churchill-laan 16 II, 1052 CD, Amsterdam
            </div>
            <!--end::Description-->
          </div>
          <!--end::Address-->
        </div>
        <!--end::Col-->
      </div>
      <!--end::Row-->

      <!--begin::Card-->
      <div class="card mb-4 bg-light text-center">
        <!--begin::Body-->
        <div class="card-body py-12">
          <!--begin::Icon-->
          <a href="#" class="mx-4">
            <img
              src="/metronic8/demo2/assets/media/svg/brand-logos/facebook-4.svg"
              class="h-30px my-2"
              alt=""
            />
          </a>
          <!--end::Icon-->

          <!--begin::Icon-->
          <a href="#" class="mx-4">
            <img
              src="/metronic8/demo2/assets/media/svg/brand-logos/instagram-2-1.svg"
              class="h-30px my-2"
              alt=""
            />
          </a>
          <!--end::Icon-->

          <!--begin::Icon-->
          <a href="#" class="mx-4">
            <img
              src="/metronic8/demo2/assets/media/svg/brand-logos/github.svg"
              class="h-30px my-2"
              alt=""
            />
          </a>
          <!--end::Icon-->

          <!--begin::Icon-->
          <a href="#" class="mx-4">
            <img
              src="/metronic8/demo2/assets/media/svg/brand-logos/behance.svg"
              class="h-30px my-2"
              alt=""
            />
          </a>
          <!--end::Icon-->

          <!--begin::Icon-->
          <a href="#" class="mx-4">
            <img
              src="/metronic8/demo2/assets/media/svg/brand-logos/pinterest-p.svg"
              class="h-30px my-2"
              alt=""
            />
          </a>
          <!--end::Icon-->

          <!--begin::Icon-->
          <a href="#" class="mx-4">
            <img
              src="/metronic8/demo2/assets/media/svg/brand-logos/twitter.svg"
              class="h-30px my-2"
              alt=""
            />
          </a>
          <!--end::Icon-->

          <!-- begin::Icon -->
          <a href="#" class="mx-4">
            <img
              src="/metronic8/demo2/assets/media/svg/brand-logos/dribbble-icon-1.svg"
              class="h-30px my-2"
              alt=""
            />
          </a>
          <!--end::Icon-->
        </div>
        <!--end::Body-->
      </div>
      <!--end::Card-->
    </div>
    <!--end::Body-->
  </div>
</template>

<script setup lang="ts"></script>

<style scoped></style>
