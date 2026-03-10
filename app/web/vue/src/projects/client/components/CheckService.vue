<template>
  <div class="check-service" v-if="!checkFinish">
    Check Service<span class="spinner-border text-primary" role="status"></span>
  </div>
</template>

<script type="ts" setup>
import { onMounted, ref } from "vue";
import { axiosInstance as axios } from "@/core/services/api.client";

const domainSites =
{
  "ver": "10040",
  "domains": {
    "client.localhost": "local",
    "client-portal.thebcr.com": "au",
    "crm.bvi.thebcr.dev": "bvi",
    "cfds-portal.thebcr.com": "bvi",
    "portal.thebcr.com": "bvi",
    "client-portal.thebcrmn.com": "mn",
    "portal.thebcr.co": "cn",
    "crm.staging.thebcr.dev": "staging",
    "hub.bacclient.com": "cn",
    "portal.thebcrsea.com": "vn",
    "client.mybcr.dev": "develop",
  },
  "sites": {
    "local": {
      "siteId": 0,
      "services": [
        "https://demo.localhost:5000",
      ]
    },
    "develop": {
      "siteId": 0,
      "services": [
        "https://pro.t.api.mybcr.dev"
      ]
    },
    "staging": {
      "siteId": 0,
      "services": [
        "https://api.staging.thebcr.dev"
      ]
    },
    "bvi": {
      "siteId": 1,
      "services": [
        "https://api.bacclient.com",
        "https://api.bvi.thebcr.com",
        "https://api.thebcrsea.com",
        "https://api.au.thebcr.com",
        "https://api.thebcrmn.com",
        "https://api.cf.thebcr.com",
        "https://api.thebcr.co"
      ]
    },
    "cn": {
      "siteId": 3,
      "services": [
        "https://api.bacclient.com",
        "https://api.bvi.thebcr.com",
        "https://api.thebcrsea.com",
        "https://api.au.thebcr.com",
        "https://api.thebcrmn.com",
        "https://api.cf.thebcr.com",
        "https://api.thebcr.co"
      ],
      "ipService": "http://pv.sohu.com/cityjson?ie=utf-8"
    },
    "au": {
      "siteId": 2,
      "services": ["https://api.bacclient.com",
        "https://api.bvi.thebcr.com",
        "https://api.thebcrsea.com",
        "https://api.au.thebcr.com",
        "https://api.thebcrmn.com",
        "https://api.cf.thebcr.com",
        "https://api.thebcr.co"]
    },
    "mn": {
      "siteId": 7,
      "services": ["https://api.bacclient.com",
        "https://api.bvi.thebcr.com",
        "https://api.thebcrsea.com",
        "https://api.au.thebcr.com",
        "https://api.thebcrmn.com",
        "https://api.cf.thebcr.com",
        "https://api.thebcr.co"]
    },
    "tw": {
      "siteId": 4
    },
    "vn": {
      "siteId": 5,
      "language": "vi-vn",
      "services": [
        "https://api.bacclient.com",
        "https://api.bvi.thebcr.com",
        "https://api.thebcrsea.com",
        "https://api.au.thebcr.com",
        "https://api.thebcrmn.com",
        "https://api.cf.thebcr.com",
        "https://api.thebcr.co"]
    },
    "jp": {
      "siteId": 6
    },
    "my": {
      "siteId": 8
    },
    "all": {
      "siteId": 0,
      "language": "en-us",
      "services": [
        "https://api.bacclient.com",
        "https://api.bvi.thebcr.com",
        "https://api.thebcrsea.com",
        "https://api.au.thebcr.com",
        "https://api.thebcrmn.com",
        "https://api.cf.thebcr.com",
        "https://api.thebcr.co"
      ]
    },
    "defaultService": "https://api.bvi.thebcr.com",
    "defaultIPSservice": "https://api.bvi.thebcr.com/api/v1/geoip/country/current"
  }
};
const apis = ref({});
const now = ref(new Date().getTime());
const status = ref("checking");
const timer = ref(0);
const domain = ref("");
const site = ref("");
const fastApi = ref("");
const fastTime = ref(0);
const checkFinish = ref(false);

onMounted(() => {
  let api = window.localStorage.getItem("ServiceUrl");
  if(api != null){
    let ver = window.localStorage.getItem("ver");
    if(ver == domainSites.ver){
      // checkFinish.value = true;
    }else{
      window.localStorage.removeItem("ServiceUrl");
      window.localStorage.removeItem("SiteId");
      window.localStorage.removeItem("ver");
    }
  }
  domain.value = window.location.hostname;
  if(domainSites.domains[domain.value] == undefined){
    site.value = "bvi";
  }else{
    site.value = domainSites.domains[domain.value];
  }
  window.localStorage.setItem("SiteId", domainSites.sites[site.value].siteId);

  if(domainSites.sites[site.value].services != undefined){
    // if(domainSites.sites[site.value].services.length > 1){
      let i = 1;
      domainSites.sites[site.value].services.forEach(service => {
        checkApiService(service, domainSites.sites[site.value].siteId, i++);
      });
    // }
  }
  window.setTimeout(updateTimer,  100);
});

const updateTimer = () => {
  now.value = new Date().getTime();
  timer.value += 100;
  if(timer.value > 1000){
    if(status.value == "online"){
      checkFinish.value = true;
      window.localStorage.setItem("ServiceUrl", fastApi.value);
      window.localStorage.setItem("ver", domainSites.ver);
      fetch("/result.php", {
          method: "POST",
          headers: {
              'Accept': 'application/json',
              'Content-Type': 'application/json'
          },
          body: JSON.stringify({
              ver: domainSites.ver,
              status: "online",
              portal: true,
              result: apis.value,
              highService: fastApi.value,
          }),
      });
      return;
    }
    if(timer.value > 5000){
      status.value = "maintain";
      checkFinish.value = true;
      fetch("/result.php", {
          method: "POST",
          headers: {
              'Accept': 'application/json',
              'Content-Type': 'application/json'
          },
          body: JSON.stringify({
              ver: domainSites.ver,
              status: "maintain",
              portal: true,
              result: apis.value,
              highService: fastApi.value,
          }),
      });
      return;
    }
  }
  window.setTimeout(updateTimer,  100);
}

const checkApiService = (api, siteId, index) =>{
  apis.value[api] = {
    "api": api,
    "startTime": new Date().getTime(),
    "status": "checking",
    "endTime": 0,
    "index": index,
    "result": 0,
  };
  fetch(api + "/api/status/ping").then(response => {
    if(response.status == 200){
      apis.value[api].status = "online";
      if(fastTime.value == 0){
        fastTime.value = new Date().getTime() - apis.value[api].startTime;
        fastApi.value = api;
      } else if( new Date().getTime() - apis.value[api].startTime < fastTime.value){
        fastTime.value = new Date().getTime() - apis.value[api].startTime;
        fastApi.value = api;
      }
      apis.value[api].result = new Date().getTime() - apis.value[api].startTime;
      status.value = "online";
    }else{
      apis.value[api].status = "offline";
    }
    apis.value[api].endTime = new Date().getTime();
  });
}
</script>
<style>
.check-service {
  position: absolute;
  bottom: 10px;
  right: 10px;
}
</style>
