let idx = 8;
const projectIds = GetProjectIds();

function appendLoadItemClass(projects) {
  const projectLimit = document.getElementById('projectCount').value;
  if (projectLimit > 4 && idx >= 8) {
    projects.forEach((project) => {
      project.classList.add('load-item');
      project.classList.remove('on-queue');
      idx += 4;
    });
  }
}

function GetProjectIds() {
  const projectIds = [];
  const projectLimit = document.getElementById('projectCount').value;
  for (let i = 8; i < projectLimit; i++) {
    projectIds.push(`#loadItem${i}`);
  }
  return projectIds;
}

function getFourProjectIds() {
  let fourProjectIds = null;
  if (projectIds < 4) {
    fourProjectIds = projectIds;
  } else {
    fourProjectIds = projectIds.splice(0, 4);
  }
  projectIds.filter((projectId) => {
    return projectId !== fourProjectIds;
  });
  return fourProjectIds;
}
